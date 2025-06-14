# CatSystem2Tool

the tool can unpack/pack archives ending with .int for the CatSystem engine  

## Usage:  
Extarct Resources to directory : toolName -e \<target archive\> \<output path\> \<name mapping string\> [regex filter]  
Create Resources to archive : toolName -c \<resources directory\> \<create archive path\> \<name mapping string\> [data encrypt mtseed](e.g. 114514 or 0x1BF52)

About name mapping string : CatSystem2 will map file name based on it  
and the string is created by game exe, you can get it using [Garbro](https://github.com/crskycode/GARbro#) (or your reverse engineering tool)   
maybe you can get it with this tool in future, but i can't guarantee, i'm a slacker  