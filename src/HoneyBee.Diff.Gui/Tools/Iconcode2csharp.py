#-*- coding:utf-8 -*-


def GetCodePoints():
    codes = ""
    with open("../Fonts/MaterialIcons-Regular.codepoints","r") as f:
        lines = f.readlines()
        for line in lines:
            args = line.split(' ')
            key = args[0].strip().replace('\n','').replace('\r','')
            value = args[1].strip().replace('\n','').replace('\r','')
            codes += "\t\tpublic const int Material_"+key+"=0x"+value+";\n"
    return codes


def AppendToCsharp(codes):
    code = ""
    with open("./Icon.Material.cs.txt","r") as f:
        template = f.read().replace("//","");
        code = template.replace("{template}",codes)

    with open("./Icon.Material.cs","w") as f:
        f.write(code)
        print(code)

if __name__ == "__main__":
    codes = GetCodePoints()
    AppendToCsharp(codes)
