using System;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;

namespace JuliusSweetland.OptiKey.UnitTests.Services
{
    //Root folder containing user defined voice commands
    class Common
    {
        //Path in which user defined commands file are stored
        public static string CommandFileRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigurableCommandService.ApplicationDataPath);

        //Maximum delay to wait for service initialization
        public static int InitDelay = 300;

        /// <summary>
        /// Cleans folder containing user defined commands
        /// </summary>
        public static void CleanUserCustomCommands()
        {
            try
            {
                Directory.Delete(CommandFileRoot, true);
            }
            catch (DirectoryNotFoundException _) { }
        }
    }
        
    [TestFixture]
    class ConfigurableCommandServiceTests
    {
        /// <summary>
        /// Initialize internal properties with current language.
        /// </summary>
        [SetUp]
        public void GivenALanguage()
        {
            Common.CleanUserCustomCommands();
        }

        [Test]
        public async Task ShouldCopyDefaultCommandsToUserFile()
        {
            Settings.Default.ResourceLanguage = Enums.Languages.EnglishUK;
            var userFilePath = Path.Combine(Common.CommandFileRoot, ConfigurableCommandService.CommandFileBase + "." + Enums.Languages.EnglishUK.ToCultureInfo() + ConfigurableCommandService.CommandFileType);
            
            //Act
            var configurableCommandService = new ConfigurableCommandService();
            
            await Task.Delay(Common.InitDelay);
            Assert.That(new DirectoryInfo(Common.CommandFileRoot), Does.Exist);
            Assert.That(new FileInfo(userFilePath), Does.Exist);
            var content = File.ReadAllText(userFilePath);
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Does.Contain(FunctionKeys.Calibrate.ToString()));
        }

        [Test]
        public async Task ShouldFallbackToDefaultLocale()
        {
            Settings.Default.ResourceLanguage = Enums.Languages.GermanGermany;
            var userFilePath = Path.Combine(Common.CommandFileRoot, ConfigurableCommandService.CommandFileBase + "." + Enums.Languages.GermanGermany.ToCultureInfo() + ConfigurableCommandService.CommandFileType);

            //Act
            var configurableCommandService = new ConfigurableCommandService();

            await Task.Delay(Common.InitDelay);
            Assert.That(new DirectoryInfo(Common.CommandFileRoot), Does.Exist);
            Assert.That(new FileInfo(userFilePath), Does.Exist);
            var content = File.ReadAllText(userFilePath);
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Does.Contain(FunctionKeys.Calibrate.ToString()));
        }

        [Test]
        public async Task ShouldMergeDefaultCommandsIntoCustomOne([Random(1)] int rand)
        {
            //Arrange: custom voice commands but without the last default value
            var resourceManager = new ResourceManager(ConfigurableCommandService.DefaultPath + ConfigurableCommandService.CommandFileBase, typeof(ConfigurableCommandService).Assembly);
                
            var language = Enums.Languages.FrenchFrance;
            Settings.Default.ResourceLanguage = language;
            var userFilePath = Path.Combine(Common.CommandFileRoot, ConfigurableCommandService.CommandFileBase + "." + language.ToCultureInfo() + ConfigurableCommandService.CommandFileType);
            var content = "";
            var removed = "";
            foreach(System.Collections.DictionaryEntry entry in resourceManager.GetResourceSet(language.ToCultureInfo(), true, true))
            {
                var functionKey = StringExtensions.Parse<FunctionKeys>((string)entry.Key);
                var value = entry.Value;
                if (functionKey != FunctionKeys.YesQuestionResult) {
                    content +=  functionKey + ";" + value + "\n";
                } else {
                    removed = functionKey + ";" + value;
                }
            }
            
            var arrowUpCommand = FunctionKeys.ArrowUp.ToString() + ";Up" + rand;
            Assert.That(content, Does.Not.Contain(removed));
            Directory.CreateDirectory(Common.CommandFileRoot);
            File.WriteAllText(userFilePath, content + arrowUpCommand + "\n");

            //Act
            var configurableCommandService = new ConfigurableCommandService();

            await Task.Delay(Common.InitDelay * 2);
            Assert.That(new DirectoryInfo(Common.CommandFileRoot), Does.Exist);
            Assert.That(new FileInfo(userFilePath), Does.Exist);
            content = File.ReadAllText(userFilePath);
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Does.Contain(arrowUpCommand));
            Assert.That(content, Does.Contain(removed));
        }
    }

    [TestFixture]
    class ConfigurableCommandServiceAsyncTests
    {
        IConfigurableCommandService configurableCommandService;
        Languages Language = Enums.Languages.EnglishUK;
        string UserFilePath;

        /// <summary>
        /// Init service and awaits for language loading.
        /// </summary>
        /// <returns></returns>
        [OneTimeSetUp]
        public async Task GivenInitializedService()
        {
            GivenALanguage();
            Common.CleanUserCustomCommands();
            configurableCommandService = new ConfigurableCommandService();
            await Task.Delay(Common.InitDelay);
        }

        /// <summary>
        /// Initialize internal properties with current language.
        /// </summary>
        [SetUp]
        public void GivenALanguage()
        {
            Settings.Default.ResourceLanguage = Language;
            UserFilePath = Path.Combine(Common.CommandFileRoot, ConfigurableCommandService.CommandFileBase + "." + Language.ToCultureInfo() + ConfigurableCommandService.CommandFileType);
        }

        [Test]
        public void ShouldSaveUserCustomCommands([Random(1)] int rand)
        {
            var arrowDownCommand = "Down" + rand;
            configurableCommandService.Commands.Add(FunctionKeys.ArrowDown, arrowDownCommand);

            //Act
            configurableCommandService.Save(Language);
            
            Assert.That(new FileInfo(UserFilePath), Does.Exist);
            var content = File.ReadAllText(UserFilePath);
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Does.Contain(arrowDownCommand));
        }

        [Test]
        public void ShouldReadUserCustomCommands([Random(1)] int rand)
        {
            var content = File.ReadAllText(UserFilePath);
            var arrowUpCommand = "Up" + rand;
            File.WriteAllText(UserFilePath, content + FunctionKeys.ArrowUp.ToString() + ";" + arrowUpCommand + "\n");

            //Act
            configurableCommandService.Load(Language);

            Assert.That(configurableCommandService.Commands, Is.Not.Empty);
            Assert.That(configurableCommandService.Commands.ContainsValue(arrowUpCommand), Is.True);
            Assert.That(configurableCommandService.Commands[FunctionKeys.ArrowUp], Is.EqualTo(arrowUpCommand));
        }

        [Test]
        public void ShouldUseGivenLocale([Random(1)] int rand)
        {
            Language = Languages.FrenchFrance;
            GivenALanguage();
            var arrowLeftCommand = "Gauche" + rand;
            File.WriteAllText(UserFilePath, FunctionKeys.ArrowLeft.ToString() + ";" + arrowLeftCommand + "\n");

            //Act
            configurableCommandService.Load(Language);

            //Assert
            Assert.That(configurableCommandService.Commands, Is.Not.Empty);
            Assert.That(configurableCommandService.Commands.ContainsValue(arrowLeftCommand), Is.True);
            Assert.That(configurableCommandService.Commands[FunctionKeys.ArrowLeft], Is.EqualTo(arrowLeftCommand));
        }
    }
}
