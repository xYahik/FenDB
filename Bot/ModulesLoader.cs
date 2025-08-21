using System.Reflection;

public class ModulesLoader
{
    public List<IModules> _modules;

    public async Task InitializeModules()
    {
        _modules = LoadModules();
        foreach (var module in _modules)
        {
            await module.Initialize();
        }
    }

    private List<IModules> LoadModules()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var modulesTypes = assembly.GetTypes()
            .Where(t =>
                typeof(IModules).IsAssignableFrom(t) &&
                t.IsClass &&
                !t.IsAbstract
            );

        var modules = modulesTypes
            .Select(Activator.CreateInstance)
            .Cast<IModules>()
            .ToList();

        return modules;
    }
}