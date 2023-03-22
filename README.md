# AIVoice
English voice to Japanese Ai Voice for Discord/Games.

# Showcase
https://www.youtube.com/watch?v=tyvXOsiatW4


# Installation

Before doing any of the steps, make sure that you have **.NET Core 6 SDK** installed so that the application can run
https://dotnet.microsoft.com/en-us/download


You're going to have to need to get a couple of API keys to work with this application:

* **Step 1** Head to https://platform.openai.com/account/api-keys and log in/sign up.
* **Step 2** Click "Create new secret key" **and save it because you'll need it when you use the application for the first time.**

* **Step 3** Head to https://www.deepl.com/pro-api?cta=header-pro-api/ and click "Sign Up For Free".
* **Step 4** Obtain your API Key **and save it because you will need it when you use the application for the first time.**

**IMPORTANT:** If you can't use DeepL since it's not avaliable in your country, head to the **Usage Without DeepL** section before continuing the steps.

* **Step 5**: Install **Virtual Cable** from https://vb-audio.com/Cable/
* **Step 6**: Open your Sound settings and go to Recording. Select the **Virtual Cable**, and click Properties. Click the listen tab, and check "Listen to this Device" and make sure that "Playback through this device" is selected as your headphones/speakers.
* **Step 7**: Make sure that your microphone is set as the Default Device and your speaker/headphones as well.
* **Step 8**: Download VoiceVox from https://voicevox.hiroshiba.jp/
* **Step 9**: Download the application from https://github.com/Sound932/AIVoice/releases
* **Step 10**: Open up **VOICEVOX**, and then open up the application, enter your API keys, and enjoy the application.

**If you're stuck on any step, feel free to hit me up on Discord(username in profile), and I'll help you set it up.**

# Usage Without DeepL
If you can't use DeepL for the fact that it's not avaliable in your country, there is a separate release that doesn't uses a different api than DeepL. It doesn't have DeepL **informal Japanese** translation quality, but it has good **informal Japanese** translation quality that it shouldn't be a problem/noticeable.

Download the release that has "**WithoutDeepL**" in its name, and then continue the installation steps from step 5. 

# Why do I not see Japanese text on the console
Console applications by default use a font that doesn't support unicode, so you're going to have use a font that supports unicode. 

Right click the console and go to properties, then head to Font and choose NSimSun and that should display Japanese characters

**If you're stuck on any step, feel free to hit me up on Discord(username in profile), and I'll help you set it up.**

# Using it in Discord/Games

Make sure that whenever you want to use it in Discord/Games is that the microphone is set as the Virtual Cable and not your own microphone.

# APIs used
* OpenAi Audio Transcription
* VOICEVOX
* DeepL
