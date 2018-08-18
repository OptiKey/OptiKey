using System.Speech.Synthesis;

/**
 * This is a sample OptiKey extension DLL. You will find here 2 very simple examples:
 * 
 * 1) Method "SayHello": This example just says something using windows TTS system.
 * 2) Method "WeGotAnError": This example shows you how to send error messages back to OptiKey. Messages will be presented to the user in a error toast notification.
 * 
 * In order to configure "SayHello" as a action in a DynamicKeyboard, you can use the snippet below:
 * 
        <PluginKey>
            <Label>Hello!</Label> <!-- You can also use a "Symbol" node -->
            <Row>6</Row>
            <Col>2</Col>
            <Plugin>HelloWorld</Plugin>
            <Method>SayHello</Method>
            <Argument>
                <Name>whatToSay</Name>
                <Value>Hello world!</Value>
            </Argument>
        </PluginKey>
 *
 * CommandLine is composed of three parts, separated by a ':' character:
 * 1) DLL file name, with or without extension, case insensitive. In the example above, "HelloWorldExtension"
 * 2) Class name, with namespace, case sensitive. In the example above, "OptiKeyExtensions.HelloWorld"
 * 3) Method name, case sensitive. It takes no arguments and has no return type (void). In the example above "SayHello"
 *
 * Please refer to OptiKey wiki for more information on registering and developing extensions.
 */

// You must use a namespace starting with "JuliusSweetland.OptiKey.". The rest of the namespace is up to you
namespace JuliusSweetland.OptiKey.StandardPlugins
{
    // Name your class anything you want. This is the name you will use in Dynamic Keyboard key definition
    public class HelloWorld
    {
        // Initialize a new instance of the SpeechSynthesizer.
        private static SpeechSynthesizer synth = new SpeechSynthesizer();

        // Those are two required methods. Getters for internal OptiKey ID, name and description
        public string GetPluginId() => "HelloWorld";
        public string GetPluginName() => "Hello World Plugin";
        public string GetPluginDescription() => "This is a sample plugin that just says \"Hello World\" through windows TTS.";

        // Name your method anything you want. You can have as many methods as you want in a single class. You will also use the method name in Dynamic Keyboard key definition.
        public void SayHello(string whatToSay) => synth.SpeakAsync(textToSpeak: whatToSay);

        // This is a error testing method. Just raise any exception that OptiKey will get the message and show to the user
        public void WeGotAnError() => throw new System.Exception(message: "Sorry, something wrong happened!");
    }
}
