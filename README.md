# SRTGenerator
Batch command line tool to generate and translate SRT subtitle files from a single video/audio (or folder) using speech to text transcription applying improvements like timing and cleaning.

## Quick guide
At this time, only Windows releases available.
- This tool requires .NET 8 runtime and VC++ redistributable libraries (Whisper.cpp).  You can download from https://dotnet.microsoft.com/en-us/download/dotnet/8.0 and https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version
- Download the last release and decompress.
- Download ZIP requirements (3.10) or (3.10_cuda) and decompress on application **Tools** folder (use CUDA ZIP if you have an NVIDIA Cuda compatible card).
- Download the *ggml-large-v3-turbo-q8_0.bin* model from https://huggingface.co/ggerganov/whisper.cpp and move it to application **Models** folder.
- Download the *vosk-model-ja-0.22.zip* from https://alphacephei.com/vosk/models and decompress on application **Models** folder. *(Optional if you want to use Vosk engine)*
- Run the application to transcribe (japanese video/audio by default) from CMD or Powershell with: 

```
.\SRTGenerator generate --input "path_of_your_video_or_folder"
```

- For more info, keep reading.
  
## Requirements
There are some requirements to properly use this tool. You can get this requirements on Releases page.
- **Python**: If you don't want to mess with your Python installation (or you don't want to install it), I've included a ZIP portable version with all necessary files. You only need one of below. ZIP package includes Python 3.10 portable and all the required modules to work properly with the command line tool (demucs, silero, argos-translate, and FFMpeg executables on python directory).
    - (3.10): For CPU or Vulkan version.
    - (3.10_cuda): For users with an NVIDIA Cuda compatible card (recommended).
- **Whisper cpp model**: To use this engine to transcribe (recommended) you need to download a model from https://huggingface.co/ggerganov/whisper.cpp and update appsettings.json (see Settings section)
- **Vosk language model**: To use this engine to transcribe you need to download and decompress the language model from https://alphacephei.com/vosk/models and update appsettings.json (see Settings section)

The next requirements are only for the translation engines and are optional.
- **Google**: If you want to translate subtitles using this engine, you need a key.json file from your Google Cloud development account and update appsettings.json (search how to create an account and manage your API keys)
- **DeepL**: If you want to translate subtitles using this engine, you need a DeepL API key and update appsettings.json (search how to create a DeepL account and create your API key for free).

## How to use
This command line tool comes with two actions:
- **generate**: This command transcribes a single video/audio or folder generating an SRT subtitle file.  The next arguments are available:
    - --input "VideoAudioFileOrFolder": Video/audio file to convert or folder containing files to process.
    - --model "WhisperModel": Overrides the default Whisper model without update settings.
    - --cuda: Use this argument if you have an NVIDIA Cuda compatible card to improve performance.
    - --split "minutes": Split audio to process in minutes (prevents memory errors on demucs - low RAM devices)
    - --engines "whisper": Default "whisper". Space separated engines to use on subtitle generation. Available engines: whisper, vosk.
    - --language "japanese": Default "japanese". Spoken language of video/audio.
    - --keep: Use this argument to keep all temporary files on work/job directory. Very useful if you want to reprocess only transcription with other model or engines.
    - --overwrite: Use this argument if you want to overwrite existing subtitles from previous executions.  By default the tool skips transcription if subtitle already exists.
    - --help: Prints the help with available arguments.

- **translate**: This command translates a single subtitle or folder (*.jap.srt) to english.  The next arguments are available with this command:
    - --input "JapaneseSubtitleFileOrFolder": Translates a single subtitle or all *.jap.srt files on folder.
    - --engines "google": Default "google". Space separated engines used to translate. Available engines: google, deepl, argos.
    - --overwrite: Use this argument if you want to overwrite and reprocess translation when the translated subtitle already exists. By default the tool skips translation if file already exists.
    - --help: Prints the help with available arguments.

## Settings
By default, appsettings.json comes with preconfigured settings (except for Google and DeepL sections).  If you follow Requirements section you can check if all the path and settings are in order.  All paths can be written as a relative path from the command line tool folder.
- **Tools**:
    - *PythonExe*: Path and filename of the Python executable.
      *TorchCacheFolder*: You can define this setting if you want to use the tool completely offline (except for Google and DeepL translations).
    - *FFMpegExe*: Path and filename of the FFMpeg executable.  It must be on the same Python executable folder (required for demucs).
- **Whisper**:
    - *Models*: Object key-value list. Key is the language and value is the Whisper model to use with this language.  You can use "default" as language for all non-defined languages.
- **Vosk**:
    - *Models*: Object key-value list. Key is the language and value is the Vosk model to use with this language. You can use "default" as language for all non-defined languages but Vosk models are language related.
- **DeepL**:
    - *Key*: API Key for translating using DeepL engine.
- **Google**:
    - *KeyJSON*: Path and filename of the key.json file for translating using Google engine.

## How it works
This batch command line just executes a set of tools to improve speech to text transcription and generate subtitles.
- On the first step, the tool extracts audio (if needed) using ffmpeg.
- After that, isolates vocals using python demucs.
- To improve timings, the tool separates vocals on chunks using silero.
- Finally, it transcribes every chunk using available engines (whisper, vosk).
  
Every step generates temporal files on *work/jobname* folder (jobname is sanitized video/audio file name). When the process fails, the temporal files still remain (or if you use the keep argument). This makes that non-failure steps will be skipped on the next try.

Aditionally, after all you can translate subtitles using Google, DeepL or Argos engines.

## Troubleshooting
Some problems you may encounter on low profile PCs:
- Demucs is very RAM demanding so, if you have 8GB RAM or less, use split option to split audios to 20-30 minutes.
- On low profile PCs transcription process takes x5 or more length of video/audio duration, so, be patience.
- Vosk engine is faster than Whisper but less accurate in most cases.
- Any other problem, please open an Issue to review.

## Thanks
This command line tool only simplifies and improves transcription and translation but all the creators of the included scripts and tools are the real heroes.
I don't remember where (and who wrote) silero.py script and the article about the transcription cleanning and timing improvements but my great thanks to you.
For the authors: If you don't want to see your creation here, just let me know.
Thanks to all.
