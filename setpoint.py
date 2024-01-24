import os

directory = os.getcwd()
    
for file in os.listdir(directory):
    filename = os.fsdecode(file)
    if filename.endswith(".json"): 
        print(filename)
        with open(filename, "r") as f:
            contents = f.readlines()

        for i in range(len(contents)-1):
            if '"priority"' in contents[i] and "setpointA" not in contents[i+1]:
                beg = contents[i][0: contents[i].find('"')]
                contents.insert(i+1, beg + '"setpointA": 1.0,\n')

        with open(filename, "w") as f:
            f.writelines(contents)