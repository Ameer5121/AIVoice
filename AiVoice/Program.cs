using NAudio;
using NAudio.Wave;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Diagnostics;

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
        private static string _audioFileLocation;
        private static string? _deepLAPIKey;
        private static string? _openAiAPIKey;
        private static string _keyLocation;

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
        }
        static async Task Main(string[] args)
        {
            if (!TryLoadAPIKeys()) InsertAPIKeys();
            if (!HasRequiredApplications())
            {
                Console.ReadLine();
                return;
            }    
            await Start();
        }

        private static async Task Start()
        {
            Console.WriteLine("You can start recording by pressing L, and stop recording by pressing L again. \n");
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.L)
                {
                    if (!_recording)
                    {
                        _waveFileWriter = new WaveFileWriter(_tempLocation, _waveInEvent.WaveFormat);
                        _waveInEvent.StartRecording();
                        _recording = true;
                        Console.WriteLine("Recording...");
                    }
                    else
                    {
                        _waveInEvent.StopRecording();
                        _recording = false;
                        Console.WriteLine("Recording Stopped.");
                        _waveFileWriter.Dispose();
                        string japaneseMessage = "";
                        try
                        {
                            var englishMessage = await SpeechToEnglishText(_tempLocation);
                            japaneseMessage = await EnglishToJapaneseText(englishMessage);
                        }catch(HttpRequestException)
                        {            
                            Console.WriteLine("An unexpected error has occured. Please make sure that the API keys that you inserted are correct");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }
                        await GetAudioFile(japaneseMessage);
                        await PlayAudioFile();
                        Console.WriteLine("\n\n");
                    }
                }
            }
        }

        private static void WriteData(object? sender, WaveInEventArgs e)
        {
            _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private static async Task<string> SpeechToEnglishText(string audioFile)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiAPIKey);
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("DeepL-Auth-Key", _deepLAPIKey);
            var dic = new Dictionary<string, string>();
            dic.Add("target_lang", "JA");
            dic.Add("text", text);
            var response = await _httpClient.PostAsync("https://api-free.deepl.com/v2/translate", new FormUrlEncodedContent(dic));
            if (response.StatusCode != HttpStatusCode.OK) throw new HttpRequestException();
            var japaneseText = await response.Content.ReadAsStringAsync();
            var messageModel = new
            {
                translations = new[]
                {
                   new
                   {
                       text = ""
                   }
                }
            };
            messageModel = JsonConvert.DeserializeAnonymousType(japaneseText, messageModel);
            Console.WriteLine(messageModel!.translations[0].text);
            return messageModel.translations[0].text;
        }

        private static async Task GetAudioFile(string japaneseText)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("text", japaneseText);
            dic.Add("speaker", "1");
            var queryResponse = await _httpClient.PostAsync($"http://localhost:50021/audio_query?text={japaneseText}&speaker=1", null);
            var queryJson = await queryResponse.Content.ReadAsStringAsync();
            var audioResponse = await _httpClient.PostAsync($"http://localhost:50021/synthesis?speaker=1", new StringContent(queryJson, Encoding.UTF8, "application/json"));
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

        private static Guid GetCorrectDeviceGuid()
        {
            var devices = DirectSoundOut.Devices;
            foreach (var device in devices)
            {
                if (device.Description == "CABLE Input (VB-Audio Virtual Cable)") return device.Guid;
            }
            return Guid.Empty;
        }

        private static bool VirtualCableExists() => GetCorrectDeviceGuid() == Guid.Empty ? false : true;
        private static bool VoiceVoxOn()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes) if (process.ProcessName == "VOICEVOX") return true;
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

        private static void InsertAPIKeys()
        {
            Console.WriteLine("Enter your OpenAi Audio Transcription API Key");
            _openAiAPIKey = Console.ReadLine()!.Replace(" ", "");
            Console.WriteLine("\n");
            Console.WriteLine("Enter your DeepL API key");
            _deepLAPIKey = Console.ReadLine()!.Replace(" ", "");
            Console.WriteLine("\n");
            SaveAPIKeys(_openAiAPIKey, _deepLAPIKey);
        }

        private static void SaveAPIKeys(string? openAiKey, string? DeepLKey)
        {
            using(var stream = File.CreateText("./Keys.txt"))
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
                        _openAiAPIKey = keys[0];
                        _deepLAPIKey = keys[1];
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
    }
}