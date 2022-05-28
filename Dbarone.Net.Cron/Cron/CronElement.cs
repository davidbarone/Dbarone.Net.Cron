namespace Dbarone.Net.Cron;
using System.Text.RegularExpressions;
using Dbarone.Net.Extensions.Reflection;

public abstract class CronElement {

    /// <summary>
    /// The Regex pattern defining the cron element.
    /// </summary>
    protected abstract Regex Pattern { get; }

    /// <summary>
    /// The priority of the CronElement when matching to string arguments.
    /// CronElement subclasses with lower priority values are evaluated first.
    /// </summary>
    public abstract int Priority { get; }

    /// <summary>
    /// Returns a Regex Match object for a given string input. 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    protected Match Match(string str) {
        var match = Pattern.Match(str);
        return match;
    }

    /// <summary>
    /// Returns true if the input string is a match for the current cron element.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public bool IsMatch(string str) {
        return this.Match(str).Success;
    }

    /// <summary>
    /// Hydrates a cron element based on the string input.
    /// </summary>
    /// <param name="str">The string input.</param>
    /// <param name="min">The minimum permitted value.</param>
    /// <param name="max">The maximum permitted value.</param>
    /// <returns></returns>
    protected abstract void Hydrate(string str, int min, int max);

    public abstract bool Validate(int value);

    /// <summary>
    /// Creates a new cron element based on the string input.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="min">The minimum permitted value.</param>
    /// <param name="max">The maximum permitted value.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CronElement Create(string str, int min, int max){
        var currentDomain = AppDomain.CurrentDomain;
        var cronElementTypes = currentDomain.GetSubclassTypesOf<CronElement>();
        foreach (var elementType in cronElementTypes){
            var element = Activator.CreateInstance(elementType) as CronElement;
            if (element is null) {
                throw new Exception($"CronElement.Create(): Invalid element type: {elementType.Name}.");
            }
            if (element.IsMatch(str)){
                element.Hydrate(str, min, max);
                return element;
            }
        }
        throw new Exception($"CronElement.Create(): No CronElement type matches string: {str}.");
    }
}