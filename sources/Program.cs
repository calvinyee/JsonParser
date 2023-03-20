﻿using Newtonsoft.Json;

if (args.Length < 2)
{
    Console.WriteLine("Set input and output filenames like: JsonParser.exe input.txt output.txt");
    Console.ReadLine();
    return;
}

var input = args[0];
var output = args[1];
if (!File.Exists(input))
{
    Console.WriteLine(input + " is not found");
    Console.ReadLine();
    return;
}
var lines = File.ReadAllLines(input);

Item item = new Item() { name= "", tagType="Provider"};

foreach (var line in lines)
{
    var words = line.Split(',');
    if (words.Length < 30)
    {
        continue;
    }
    var names = words[1].Split('\\');
    if (names.Length < 2)
    {
        continue;
    }
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

    if (item.tags == null)
    {
        item.tags = new List<object>();
    }
    if (newSubItem)
    {
        item.tags.Add(subItem);
    }
    List<Alarm>? alarms = null;
    if (words[6] != "F")
    {
        alarms = new List<Alarm>
        {
            new Alarm() { name = names[1], notes = words[2], label = words[18], displayPath = new DisplayPath()  }
        };
    }
    subItem.tags.Add(new Tag() { name = names[1], 
        opcItemPath = string.Format(@"ns\u003d1;s\u003d[{0}]{1}", "M30_PLC5", words[23]),
        alarms = alarms});
}

var json = JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore});
File.WriteAllText(output, json.Replace("\\\\", "\\"));

Console.WriteLine(output + " was created successfully");
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