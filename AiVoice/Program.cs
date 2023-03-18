using NAudio;
using NAudio.Wave;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
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
        private static string _location;
        private static bool _recording;
        private static string _audioFileLocation;

        static Program()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            _audioFileLocation = $"./AiAudio.wav";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "APIKEY");
            _httpClient.DefaultRequestHeaders.Add("X-Naver-Client-Id", "CLIENTID");
            _httpClient.DefaultRequestHeaders.Add("X-Naver-Client-Secret", "SECRET");
            _location = $"{Environment.CurrentDirectory}Temp.wav";
            _waveInEvent = new WaveInEvent();
            _waveOutEvent = new DirectSoundOut(GetCorrectDeviceGuid());
            _waveOutEvent.Volume = 1;
            _waveInEvent.DataAvailable += WriteData;
        }
        static async Task Main(string[] args)
        {
            if (!VirtualCableExists())
            {
                Console.WriteLine("VB Virtual Cable is not installed or is disabled. Please check it before starting.");
                return;
            }else if (!VoiceVoxOn())
            {
                Console.WriteLine("VoiceVox is not installed or is not open. Please check it before starting.");
                return;
            }
            Console.WriteLine("You can start recording by pressing L, and stop recording by pressing L again. \n");
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.L)
                {
                    if (!_recording)
                    {
                        _waveFileWriter = new WaveFileWriter(_location, _waveInEvent.WaveFormat);
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
                        var englishMessage = await SpeechToEnglishText(_location);
                        var japaneseMessage = await EnglishToJapaneseText(englishMessage);
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
            var bytes = File.ReadAllBytes(audioFile);
            var formData = new MultipartFormDataContent();
            formData.Add(new ByteArrayContent(bytes), "file", "wav");
            formData.Add(new StringContent("whisper-1"), "model");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", formData);
            var jsonMessage = await response.Content.ReadAsStringAsync();
            var messageModel = new
            {
                text = ""
            };
            messageModel = JsonConvert.DeserializeAnonymousType(jsonMessage, messageModel);
            Console.WriteLine(messageModel.text);
            return messageModel.text;
        }

        private static async Task<string> EnglishToJapaneseText(string text)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("source", "en");
            dic.Add("target", "ja");
            dic.Add("text", text);
            var response = await _httpClient.PostAsync("https://openapi.naver.com/v1/papago/n2mt", new FormUrlEncodedContent(dic));
            var japaneseText = await response.Content.ReadAsStringAsync();
            var messageModel = new
            {
                message = new
                {
                    result = new
                    {
                        translatedText = ""
                    }
                }
            };
            messageModel = JsonConvert.DeserializeAnonymousType(japaneseText, messageModel);
            Console.WriteLine(messageModel.message.result.translatedText);
            return messageModel.message.result.translatedText;
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
    }
}