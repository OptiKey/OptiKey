# Using the HunSpell and OpenOffice Dictionary Files

1. Download HunSpell from here: [https://sourceforge.net/projects/ezwinports/files/]()  
e.g. [https://sourceforge.net/projects/ezwinports/files/hunspell-1.3.2-3-w32-bin.zip/download]()  
This gives you the `munch.exe` and `unmunch.exe` executables in the `bin` directory.  
2. Download the dictionaries. There are two options to do this, outlined below.  
    1. Dictionaries can be downloaded from here: [http://extensions.services.openoffice.org/en/search?f[0]=field_project_tags%3A157]()  
    e.g. [http://extensions.services.openoffice.org/en/project/hellenic-greek-dictionary-spell-check-and-hyphenation]()  
    This gives you an `.oxt` file - open it as an archive (using [7zip](https://www.7-zip.org/download.html) for example) and extract the culture code `.dic` and `.aff` files.  
    2. Alternatively you can download (what seem to be) the same Hunspell compatible dictionaries from SoftMaker here: [http://www.softmaker.com/en/download/dictionaries]()  
3. Unmunch the dictionary files, using this command format: `unmunch el-GR.dic el-GR.aff >> el-GR.unmunched`  
4. Open and inspect the unmunched file. Using the result from step #3, the file to open is `el-GR.unmunched`.  
*N.B.* You may have to manually open the file using the correct encoding and resave as UTF-8. Greek, for example, is gibberish when you first open the unmunched file in Sublime Text, but re-opening using "Greek Windows" encoding solves that problem.  
5. The dictionary files can be massive, so further filtering can be performed by then extracting only entries in the frequency word lists: [https://invokeit.wordpress.com/frequency-word-lists/]()