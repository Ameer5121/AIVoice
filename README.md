# AIVoice
English voice to Japanese Ai Voice for Discord/Games.

# Showcase
https://www.youtube.com/watch?v=tyvXOsiatW4


![image](https://user-images.githubusercontent.com/71935713/227421387-932b8a50-5417-4010-8794-f7e7051cb295.png)


# Installation

Before doing any of the steps, make sure that you have **.NET Core 6 SDK** installed so that the application can run
https://dotnet.microsoft.com/en-us/download


You're going to have to need to get a couple of API keys to work with this application:

* **Step 1** Head to https://platform.openai.com/account/api-keys and log in/sign up.
* **Step 2** Click "Create new secret key" **and save it because you'll need it when you use the application for the first time.**

* **Step 3** Head to https://www.deepl.com/pro-api?cta=header-pro-api/ and click "Sign Up For Free".
* **Step 4** Obtain your API Key **and save it because you will need it when you use the application for the first time.**

**IMPORTANT:** If you can't use DeepL since it's not avaliable in your country or the fact that it needs a credit card for registration, head to the **Usage Without DeepL** section for a really simple installation before continuing the steps.

* **Step 5**: Install **Virtual Cable** from https://vb-audio.com/Cable/
* **Step 6**: On the right side of the taskbar, right click on your sound icon and click "Sounds", and then head to the Recording tab.

![image](https://user-images.githubusercontent.com/71935713/229618860-f7904bd0-be4b-43c1-8182-616b16ae05e7.png)

 Select **Virtual Cable**, and then click Properties. Head to the Listen tab, and then check "Listen to this Device". Make sure that "Playback through this device" is selected as your headphones/speakers.
 
![image](https://user-images.githubusercontent.com/71935713/229619239-6d8ce6dd-cd4b-4485-8297-dae608ad4c52.png)

* **Step 7**: Make sure that your microphone is set as the Default Device as well as your speakers/headphones.
![image](https://user-images.githubusercontent.com/71935713/229619532-810da69b-a95d-4b68-bc42-08730a5e7b7d.png) ![image](https://user-images.githubusercontent.com/71935713/229618860-f7904bd0-be4b-43c1-8182-616b16ae05e7.png)



* **Step 8**: Download VoiceVox from https://voicevox.hiroshiba.jp/.
* **Step 9**: Download the application from https://github.com/Sound932/AIVoice/releases
* **Step 10**: Open up **VOICEVOX**(Leave it in the background), and then open up the application, enter your API keys, and enjoy the application.

**If you're stuck on any step, feel free to hit me up on Discord(username in profile), and I'll help you set it up.**

# Usage Without DeepL

If you can't use DeepL for the fact that it's not avaliable in your country, follow these steps below:

* **Step 1** - Head to https://www.translate.com/users/customer_create and create an account.
* **Step 2** - Save your credentials because you're going to need them when you open the application.

**It has 80-90% of DeepL's translation quality and is almost identical. It's nowhere near as bad as Google Translate**.

# Why DeepL?
DeepL has hands down the best English to Japanese translation, and that's why it is the main API for this application. The other option that is mentioned has 70-80% of the translation quality as DeepL, and is good enough for most cases.

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
