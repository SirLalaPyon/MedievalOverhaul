import os

originalName = "ModSourceTemplate"
newModName = input("New mod name: ")
rootdir = os.getcwd()

for root, subFolders, files in os.walk(rootdir):
    for filename in files:
        if "SetUpProject" not in filename:
            oldPath = os.path.join(root, filename)
            filename = filename.replace(originalName, newModName);
            newPath = os.path.join(root, filename)
            os.rename(oldPath, newPath)

for root, subFolders, files in os.walk(rootdir):
    for filename in files:
        if "SetUpProject" not in filename:
            path = os.path.join(root, filename)
            print(path)
            with open(path) as f:
                s = f.read()
            s = s.replace(originalName, newModName)
            with open(path, "w") as f:
                f.write(s)
        
