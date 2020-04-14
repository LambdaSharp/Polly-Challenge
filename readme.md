# Amazon Polly Challenge

In this challenge, we'll be exploring Amazon Polly to convert text to audio and then building the exact case study show below:

![Amazon Polly Flow existing case study](case-study.png)

## Helpful Links

[.NET AWS Polly SDK Docs](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/index.html?page=Polly/MPollySynthesizeSpeechSynthesizeSpeechRequest.html&tocid=Amazon_Polly_AmazonPollyClient)
[.NET AWS SDK Docs](https://docs.aws.amazon.com/sdkfornet/v3/apidocs)

## Prerequisites

The following tools and accounts are required to complete these instructions.

- [Install .NET Core 3.1](https://www.microsoft.com/net/download)
- [LambdaSharp Tool](https://github.com/LambdaSharp/LambdaSharpTool)
- [AWS Account](https://aws.amazon.com/)
- [GitHub Account](https://github.com/)
- [Install AWS CLI](https://aws.amazon.com/cli/)

### Install SDK & Tools

Make sure the following tools are installed.

- [Download and install the .NET Core SDK](https://dotnet.microsoft.com/download)
- [Download and install Git Command Line Interface](https://git-scm.com/downloads)

### Setup AWS Account and CLI

The challenge requires an AWS account. AWS provides a [_Free Tier_](https://aws.amazon.com/free/), which is sufficient for most challenges.

- [Create an AWS Account](https://aws.amazon.com)

### Clone GitHub Repository

Next, you will need to clone this repo into your working directory:

```bash
git clone https://github.com/LambdaSharp/Polly-Challenge.git
```

### Setup LambdaSharp Deployment Tier

The following command uses the `dotnet` CLI to install the LambdaSharp CLI.

```bash
dotnet tool install --global LambdaSharp.Tool
```

**NOTE:** if you have installed LambdaSharp.Tool in the past, you will need to run `dotnet tool update -g LambdaSharp.Tool` instead.

The following command uses the LambdaSharp CLI to create a new deployment tier on the default AWS account. Specify another account using `--aws-profile ACCOUNT_NAME`.

```bash
lash init --quick-start
```

## Level 0

- Search for "Amazon Polly" in AWS Console
- Once in the Amazon Polly console, add some text to the "Plain Text" input area then click "Listen to speech".
- Try different "Voices" and "Language and Region" then click "Listen to speech" again.
- Ensure you can play the audio file locally by clicking "Download MP3" and opening the file.

## Level 1

- In the cloned repo working directory, Polly-Challenge, deploy LambdaSharp by running the following command: `lash deploy`. This will take a minute. 

- Fill out your phone number to receive SMS messages!  No spaces or dashes i.e 15551234567

  ```bash
  *** NOTIFICATION SETTINGS *** 
  |=> NotificationSms [String]: Phone number for PollyFunction to send SMS messages for Polly audio files:
  ```

- Once completed, navigate to the API Gateway Deployment section in the AWS Console.  Here is a link to the API Gateway console in US-EAST-1: https://console.aws.amazon.com/apigateway/main/apis?region=us-east-1
- Test Amazon Polly by sending a web request to the API Gateway endpoint like this: `curl -d '{"Content": "Hello world! This is some test content.", "FileName": "test.mp3"}' -H "Content-Type: application/json" -X POST https:/REPLACEME.execute-api.us-east-1.amazonaws.com/LATEST/text-to-audio` 
  - Be sure to replace the `REPLACEME` with the actual subdomain or entire url. Works with Git Bash, \*Nix, and Postman.

You can now navigate to the [AWS S3 console](https://s3.console.aws.amazon.com/s3/home?region=us-east-1) `ArticlesBucket` for the saved file `test.mp3`  

You can also do another curl request to get a list of files from that bucket `curl -X GET https:/REPLACEME.execute-api.us-east-1.amazonaws.com/LATEST/files`

## Level 2

Let's get notified when the converted text to audio file is ready. Send an SMS using an SNS topic. Add a link to the MP3 in the SNS message. Some setup has already been completed for you.

Docs: [Amazon SNS Publish](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/index.html?page=SNS/MSNSPublishAsyncStringStringCancellationToken.html&tocid=Amazon_SimpleNotificationService_Amaz)

## Level 3

We want the user to be able to choose a language, voice style, and get audio based on that selection. Using the `curl` request from Level 1, add a property for `LanguageCode` and `VoiceId`

Use Amazon Polly's built in localization methods to describe the voices available for that language code.

Pick a voice from that list and use it to synthesize the text to speech in the chosen language.

Submit the chosen voice in the API request.

NOTE: This should not require any text translation

[Docs] https://docs.aws.amazon.com/sdkfornet/v3/apidocs/index.html?page=Polly/MPollyDescribeVoicesDescribeVoicesRequest.html&tocid=Amazon_Polly_AmazonPollyClient

[Language_Codes] https://docs.aws.amazon.com/polly/latest/dg/SupportedLanguage.html

## Level 4

We don't want to expend processing power on duplicate files! If the content of the incoming article is identical to one that has already been saved, then do not send for audio processing but still send a link to the MP3 file in a SMS notification.

## Level 5

We want to poll an article list with title and description every 5 minutes from an RSS feed. Typically this would be done every day but for the purposes of this challenge, 5 minute intervals will work. Save the audio file in the following format `{timestamp}.mp3`. Update your lambda function to handle triggers other than API Gateway

Example RSS Feed 1: https://hnrss.org/newest

Example RSS Feed 2: https://www.reddit.com/r/news/.rss

Note: Some parsing will be required!

Like JSON better? Check this out https://rss2json.com/#rss_url=https%3A%2F%2Ftechcrunch.com%2Ffeed%2F

Check out these docs for some details on scheduling with AWS
https://docs.aws.amazon.com/lambda/latest/dg/tutorial-scheduled-events-schedule-expressions.html
https://docs.aws.amazon.com/AmazonCloudWatch/latest/events/RunLambdaSchedule.html

<details><summary>Hint</summary>
Make a post request to the API Gateway endpoint with the content from the RSS feed.
</details>
<details><summary>Not hard enough for you?</summary>
Parse the article's html into plain text then convert it to an mp3.  This could be in the field: <code>content:encoded</code>
</details>

## Boss

![Flow](thanos.jpg)

We want to be able to listen to the audio in the language of our choice. Use Amazon Transcribe to translate the text into another language before sending it to Polly.

<details><summary>Hint 1</summary>
Polly and Transcribe are similar services. Use the existing definitions in the <code>Module.yml</code> for ideas.
</details>

<details><summary>Hint 2</summary>
No, this is the boss level!
</details>
