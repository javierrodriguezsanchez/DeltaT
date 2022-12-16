namespace DeltaTEngine;
public static class Bridge
{
    public static acount PageInfo=null!;
    public static IEnumerable<string> AllUsers()
    {
        foreach (var item in Directory.GetDirectories(Path.Combine(Database,"Users")))
            foreach (var name in Directory.GetFiles(item))
                yield return Path.GetFullPath(name);
    }
    public static string Database
    {
        get
        {
            return Path.Combine("..","DeltaTEngine","Database");
        }
    }

}
