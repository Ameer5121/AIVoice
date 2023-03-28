using NAudio;
using NAudio.Wave;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace AiVoice
{
    internal class Program
    {
        private static WaveFileWriter _waveFileWriter;
        private static WaveInEvent _waveInEvent;
        private static AudioFileReader _audioFileWriter;
        private static DirectSoundOut _waveOutEvent;
        private static HttpClient _httpClient;
        private static string _tempLocation;
        private static bool _recording;
        private static bool _inSettings;
        private static string _audioFileLocation;
        private static string? _translationBearerToken;
        private static string? _openAiBearerToken;
        private static string _keyLocation;
        private static int _voice = 11;
        private static Dictionary<int, string> _characters;

        static Program()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            _keyLocation = "./Keys.txt";
            _audioFileLocation = "./AiAudio.wav";
            _httpClient = new HttpClient();
            _tempLocation = "./Temp.wav";
            _waveInEvent = new WaveInEvent();
            _waveOutEvent = new DirectSoundOut(GetCorrectDeviceGuid());
            _waveOutEvent.Volume = 1;
            _waveInEvent.DataAvailable += WriteData;
            _characters = new Dictionary<int, string>
            {
                {1, "Zundamon (Female)" },
                {11, "Takehiro Kurono (Male)"},
                {13, "Ryuusei Aoyama (Male)"},
                {20, "Mochiko (Female)"},
            };
        }
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        static async Task Main(string[] args)
        {
            if (!TryLoadAPIKeys()) await InsertCredentials();
            if (!HasRequiredApplications())
            {
                Console.ReadLine();
                return;
            }
            await Start();
        }

        private static async Task Start()
        {
            InitiateIntroduction();
            while (true)
            {
                for (int x = 0; x < 255; x++)
                {
                    var keyState = GetAsyncKeyState(x);
                    var key = (Keys)x;
                    if (keyState != 0)
                    {
                        if (key == Keys.L)
                        {
                            if (!_recording) InitiateRecord();
                            else
                            {
                                FinishRecording();
                                string japaneseMessage = "";
                                try
                                {
                                    var englishMessage = await SpeechToEnglishText(_tempLocation);
                                    japaneseMessage = await EnglishToJapaneseText(englishMessage);
                                }
                                catch (HttpRequestException) { DisplayError("API keys"); }
                                await GetAudioFile(japaneseMessage, _voice);
                                await PlayAudioFile();
                                Console.WriteLine("\n\n");
                            }
                        }
                        else if (key == Keys.O && _recording) AbortRecording();
                        else if (key == Keys.V && !_recording && !_inSettings) SetVoice();
                    }
                }
            }
        }

        private static void InitiateIntroduction()
        {
            Console.WriteLine("_____________________________________________________________________________");
            Console.WriteLine("1. You can start recording by pressing L, and finish recording by pressing L again.\n");
            Console.WriteLine("2. You can abort the recording by pressing O\n");
            Console.WriteLine("3. You can change the Ai's voice by pressing V");
            Console.WriteLine("_____________________________________________________________________________\n");
        }

        private static void InitiateRecord()
        {
            _waveFileWriter = new WaveFileWriter(_tempLocation, _waveInEvent.WaveFormat);
            _waveInEvent.StartRecording();
            _recording = true;
            Console.Beep();
            Console.WriteLine("Recording...");
        }

        private static void FinishRecording()
        {
            _waveInEvent.StopRecording();
            _recording = false;
            Console.WriteLine("Recording Stopped.");
            Console.Beep();
            _waveFileWriter.Dispose();
        }

        private static void AbortRecording()
        {
            _waveInEvent.StopRecording();
            _waveFileWriter.Dispose();
            Console.WriteLine("Aborted Recording");
            _recording = false;
        }
        private static void WriteData(object? sender, WaveInEventArgs e) => _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);

        private static async Task<string> SpeechToEnglishText(string audioFile)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiBearerToken);
            var bytes = File.ReadAllBytes(audioFile);
            var formData = new MultipartFormDataContent();
            formData.Add(new ByteArrayContent(bytes), "file", "wav");
            formData.Add(new StringContent("whisper-1"), "model");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", formData);
            if (response.StatusCode != HttpStatusCode.OK) throw new HttpRequestException();
            var jsonMessage = await response.Content.ReadAsStringAsync();
            var messageModel = new
            {
                text = ""
            };
            messageModel = JsonConvert.DeserializeAnonymousType(jsonMessage, messageModel);
            Console.WriteLine(messageModel!.text);
            return messageModel.text;
        }

        private static async Task<string> EnglishToJapaneseText(string text)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _translationBearerToken);
            var translationRequest = new
            {
                text = text,
                source_language = "en",
                translation_language = "ja",
            };
            var httpRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(translationRequest), Encoding.UTF8, "application/json")
            };
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync("https://api.translate.com/translate/v1/mt", httpRequestMessage.Content);
            if (response.StatusCode != HttpStatusCode.OK) throw new HttpRequestException();
            var japaneseText = await response.Content.ReadAsStringAsync();
            var translationModel = new
            {
                data = new
                {
                    translation = ""
                }
            };
            translationModel = JsonConvert.DeserializeAnonymousType(japaneseText, translationModel);
            Console.WriteLine(translationModel!.data.translation);
            return translationModel.data.translation;
        }

        private static async Task GetAudioFile(string japaneseText, int speaker)
        {
            var queryResponse = await _httpClient.PostAsync($"http://localhost:50021/audio_query?text={japaneseText}&speaker={speaker}", null);
            var queryJson = await queryResponse.Content.ReadAsStringAsync();
            var audioResponse = await _httpClient.PostAsync($"http://localhost:50021/synthesis?speaker={speaker}", new StringContent(queryJson, Encoding.UTF8, "application/json"));
            using (FileStream stream = new FileStream(_audioFileLocation, FileMode.Create, FileAccess.Write))
            {
                var buffer = await audioResponse.Content.ReadAsByteArrayAsync();
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private static async Task PlayAudioFile()
        {
            using (_audioFileWriter = new AudioFileReader(_audioFileLocation))
            {
                _waveOutEvent.Init(_audioFileWriter);
                _waveOutEvent.Play();
                await Task.Delay(_audioFileWriter.TotalTime);
            }
        }

        private static void SetVoice()
        {
            _inSettings = true;
            string? input = "";
            int id = 0;
            Console.WriteLine($"Current voice is {_characters[_voice]}");
            Console.WriteLine("_______________________________________\n");
            Console.WriteLine("All of the avaliable characters are listed below. Please type the ID of the character you want to use.\n");
            foreach (var character in _characters) Console.WriteLine($"{character.Key}:  {character.Value}");
            Console.WriteLine();
            ClearConsoleBuffer();
            do
            {
                input = Console.ReadLine();
            } while (!int.TryParse(input, out id) || !_characters.Any(x => x.Key == id));
            _voice = id;
            _inSettings = false;
            Console.WriteLine($"Sucessfully changed voice to {_characters[_voice]} \n\n");
        }

        private static Guid GetCorrectDeviceGuid()
        {
            var devices = DirectSoundOut.Devices;
            foreach (var device in devices)
                if (device.Description == "CABLE Input (VB-Audio Virtual Cable)") return device.Guid;
            return Guid.Empty;
        }

        private static bool VirtualCableExists() => GetCorrectDeviceGuid() == Guid.Empty ? false : true;
        private static bool VoiceVoxOn()
        {
            foreach (var process in Process.GetProcesses()) if (process.ProcessName == "VOICEVOX") return true;
            return false;
        }

        private static bool HasRequiredApplications()
        {
            if (!VirtualCableExists())
            {
                Console.WriteLine("VB Virtual Cable is not installed or is disabled. Please check it before starting.");
                return false;
            }
            else if (!VoiceVoxOn())
            {
                Console.WriteLine("VoiceVox is not installed or is not open. Please check it before starting.");
                return false;
            }
            return true;
        }

        private static async Task InsertCredentials()
        {
            Console.WriteLine("Enter your OpenAi Audio Transcription API Key");
            _openAiBearerToken = Console.ReadLine()!.Replace(" ", "");
            Console.WriteLine("\n");

            var token = "";
            try { token = await GetTranslationBearerToken(); }
            catch (HttpRequestException) { DisplayError("credentials"); }
            _translationBearerToken = token;
            Console.WriteLine("\n");
            SaveAPIKeys(_openAiBearerToken, _translationBearerToken);
        }

        private static async Task<string> GetTranslationBearerToken()
        {
            Console.WriteLine("Enter your Translation API Email");
            var email = Console.ReadLine()!.Replace(" ", "");
            Console.WriteLine("\nEnter your Translation API Password");
            var password = Console.ReadLine()!.Replace(" ", "");
            var credentials = new
            {
                email = email,
                password = password,
            };
            var httpRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(credentials), Encoding.UTF8, "application/json")
            };
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync("https://api.translate.com/translate/v1/login", httpRequestMessage.Content);
            if (response.StatusCode != HttpStatusCode.OK) throw new HttpRequestException();
            var responseObject = new
            {
                data = new
                {
                    token = ""
                }
            };
            responseObject = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), responseObject);
            return responseObject!.data.token;
        }

        private static void SaveAPIKeys(string? openAiKey, string? DeepLKey)
        {
            using (var stream = File.CreateText("./Keys.txt"))
            {
                stream.WriteLine(openAiKey);
                stream.WriteLine(DeepLKey);
            }
        }

        private static bool TryLoadAPIKeys()
        {
            if (File.Exists(_keyLocation))
            {
                Console.WriteLine("Would you like to use the previous keys that you have inserted? Y/N ");
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input?.ToLower() == "y")
                    {
                        var keys = File.ReadAllLines(_keyLocation);
                        _openAiBearerToken = keys[0];
                        _translationBearerToken = keys[1];
                        Console.WriteLine("\n");
                        return true;
                    }
                    else if (input?.ToLower() == "n")
                    {
                        Console.WriteLine("\n");
                        return false;
                    }
                }
            }
            return false;
        }

        private static void DisplayError(string subject)
        {
            Console.WriteLine($"An unexpected error has occured. Please make sure that the {subject} that you have inserted are correct");
            Console.ReadKey(true);
            Environment.Exit(1);
        }

        private static void ClearConsoleBuffer()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(true);
        }
    }
}