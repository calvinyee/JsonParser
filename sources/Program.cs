using Newtonsoft.Json;

if (args.Length < 1)
{
    Console.WriteLine("Set input filename: JsonParser.exe input.txt");
    Console.ReadLine();
    return;
}

var input = args[0];
if (!File.Exists(input))
{
    Console.WriteLine(input + " is not found");
    Console.ReadLine();
    return;
}
var lines = File.ReadAllLines(input);

Dictionary<string, Item> itemDict = new Dictionary<string, Item>();

foreach (var line in lines)
{
    bool rejected = false;
    if (line.StartsWith(";"))
    {
        continue;
    }
    var words = line.Split(',');
    if (words.Length < 30)
    {
        continue;
    }
    //if (!words[22].Contains("DTS-R"))
    //{
    //    continue;
    //}
    if (!words[23].Contains(':'))
    {
        rejected = true;
    }
    var names = words[1].Split('\\');
    if (names.Length < 2)
    {
        if (names.Length == 1)
        {
            names = new string[2] { "General", names[0] };
        }
        else
            continue;
    }

    string key = rejected ? "Non-PLC5" : words[22];

    Item item;
    if (!itemDict.ContainsKey(key))
    {
        itemDict.Add(key, new Item() { name = "", tagType = "Provider", tags = new List<object>() });
    }
    
    item = itemDict[key];

    bool newSubItem = false;
    var subItem = FindItem(item.tags, names[0]);
    if (subItem == null)
    {
        newSubItem = true;
        subItem = new Item() { name = names[0], tagType = "Folder" };
    }  
    
    if (subItem.tags == null)
    {
        subItem.tags = new List<object>();
    }     
    if (newSubItem)
    {
        item.tags.Add(subItem);
    }    

    Item? previousLevel = subItem;

    for (int i = 1; i < names.Length; ++i)
    {
        if (i == names.Length - 1)
        {
            if (names[i].Contains("_CMD") || names[i].Contains("_IND"))
            {
                // these are tags, not folders
                break;
            }
            if (words[6] != "F")
            {
                // alarm is not a folder
                break;
            }
            if (words[0] == "D")
            {
                // D means tag so ignore the last part
                break;
            }
        }
        newSubItem = false;
        var newItem = FindItem(previousLevel.tags, names[i]);
        if (newItem == null)
        {
            newSubItem = true;
            newItem = new Item() { name = names[i], tagType = "Folder" };
        }
        if (newItem.tags == null)
        {
            newItem.tags = new List<object>();
        }
        if (newSubItem)
        {
            previousLevel.tags.Add(newItem);
        }
        previousLevel = newItem;
    } 

    var almName = names[names.Length-1];
    if (!string.IsNullOrEmpty(almName))
    {
        almName += "_";
    }
    almName += "ALM";

    List<Alarm>? alarms = null;
    if (words[6] != "F")
    {
        var note = words[2];
        alarms = new List<Alarm>
        {
            new Alarm() { name = almName, notes = note, label = words[18],
                priority = (note != null && note.ToLower().Contains("fire")) ? "High" : "Medium",
                displayPath = new DisplayPath() }
        };
    }

    previousLevel.tags.Add(new Tag()
    {
        name = names[names.Length - 1],
        opcItemPath = string.Format(@"ns\u003d1;s\u003d[{0}]{1}", words[22], words[23]),
        alarms = alarms
    });
}

foreach (var pair in itemDict)
{
    var json = JsonConvert.SerializeObject(pair.Value, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
    json = json.Replace("S:", "S2:");
    File.WriteAllText(pair.Key + ".json", json.Replace("\\\\", "\\"));

    Console.WriteLine(pair.Key + ".json" + " was created successfully");
}

Console.ReadLine();

Item? FindItem(List<object>? items, string name)
{
    if (items == null)
        return null;
    foreach(var o in items)
    {
        var item = o as Item;
        if (item == null)
        {
            continue;
        }
        if (item.name == name)
        {
            return item;
        }
    }
    return null;
}

class Tag
{
    public string? valueSource { get; set; } = "opc";
    public string? opcItemPath { get; set; }
    public string? dataType { get; set; } = "Boolean";
    public List<Alarm>? alarms { get; set; }
    public string? name { get; set; }
    public string? tagType  { get; set; } = "AtomicTag";
    public string? opcServer { get; set; } = "Ignition OPC UA Server";
}

public class Alarm
{
    public float setpointA { get; set; } = 1.0f;
    public string? notes { get; set; }
    public string? name { get; set; }
    public string? label { get; set; }
    public string? priority { get; set; } = "Medium";
    public DisplayPath? displayPath { get;set;}
}

public class DisplayPath
{
    public string? bindType { get; set; } = "Expression";
    public string? value { get; set; } = "{notes}";
}


class Item
{
    public string? name { get; set; }
    public string? tagType { get; set; }
    public List<object>? tags { get; set; }
}