# AX3 Download/Deployment Software

## Getting Started

The project home page is at: [https://github.com/digitalinteraction/ax-deploy](https://github.com/digitalinteraction/ax-deploy).  

These instructions are available at: 
[https://digitalinteraction.github.io/ax-deploy](https://digitalinteraction.github.io/ax-deploy).  


### Hardware

For each deployment computer:

* Connect laptop to power.

* Connect USB dock(s) to power.

* Connect USB dock(s) to the computer over USB.

* Connect barcode scanner to the computer over USB.

* Verify that the laptop has a working internet connection in order to upload data files.


### Installing the software

If the software is not already installed, it is available from: 
[https://digitalinteraction.github.io/ax-deploy/deploy](https://digitalinteraction.github.io/ax-deploy/deploy).  To install, click the *Install* button, and run the downloaded *setup.exe* program.  
If told *Windows protected your PC* then click *More info* and *Run anyway* -- you will also need to install the *AX3 Driver* from the same site. 
Finally, you will may want a configuration settings file `config.ini` placed into the folder `%USERPROFILE%\Documents\AX3`.

**Note:** A non-installer version is available at [https://digitalinteraction.github.io/ax-deploy/deploy/noinstall](https://digitalinteraction.github.io/ax-deploy/deploy/noinstall).


### Starting the software

Double-click the *Deploy* icon from the desktop (or press the *Start* button and type `Deploy` to find the launch shortcut):

![Deploy icon](images/launch-icon.png)


### Interface Overview

The user interface is split into several areas:

![User interface](images/interface-areas.png)

1. *Configuration setting*: This should be scanned in by the barcode reader, but can be typed if needed.

2. *Configuration information*: Provides information, success messages, and error messages.

3. *Device details*: Lists each attached device grouped by status, including the device id, battery level and status detail.

4. *Diagnostic log*: Typically ignored in normal use, can be checked for further details in the event of a problem.

5. *Cloud upload progress*: which file it is uploading and how many other files are queued for upload.

6. *Available space*: the approximate number of downloads that the drive currently has capacity for.


### Device Status

The status of a device will typically be listed in the software.  The device's LED colour also indicates its state, and may be useful to determine the status of a batch of connected devices at a glance. The following table shows the possible LED indications:

| LED Colour  | Status  | Notes  |
|:------------|:--------|:-------|
| <span style="color: lightgray;"  >&#9679;&nbsp;**White**</span>                  | Downloading  | The data from the device is downloading.  You must wait for the download to complete.  Afterwards, the device will be automatically cleared then enter the charging or charged category. |
| <span style="color: cyan;"       >&#9679;&nbsp;**Cyan**</span>                   | Recharging   | The device is cleared but not yet fully charged.  Wait until the device is fully charged. |
| <span style="color: magenta;"    >&#9679;&nbsp;**Magenta**</span>                | Charged      | The is fully charged (and cleared).  This device can be configured now, or is ready to be removed for later configuration.  |
| <span style="color: cyan;"       >&#9788;&nbsp;**Flashing Cyan/Off**</span>      | Outbox       | The device is configured.  Disconnect the flashing device, scan the barcode and place in the envelope. |
| <span style="color: red;"        >&#9679;&nbsp;**Red**</span>                    | Unexpected   | This device was not expected on the dock, as it has been configured for recording and that recording has not yet finished.  If you are certain that the device was configured in error, use *Device* / *Destroy Recording* to manually clear it.  The firmware may temporarily light the LED red while it is clearing or configuring.  |
| <span style="color: dodgerblue;" >&#9679;&nbsp;**Blue**</span>                   | Error        | There was a problem with communicating with, or downloading from the device.  If it is a comms. error, press `F9`, `Y` to reset.  If it is a download error, check the drive has enough free space (waiting for uploads to complete may free-up capacity).  |
| <span style="color: gold;"       >&#9679;&nbsp;**Yellow**</span> / <span style="color: green;">&#9679;&nbsp;**Green**</span>  | USB data error  | This device is not communicating with the computer (but is powered or fully charged). Check the hub's connection and reconnect the device.  |
| <span style="color: gold;"       >&#9677;&nbsp;**Fading Yellow**</span> / <span style="color: lightgray;">&#9677;&nbsp;**Fading White**</span>  | Unclassified* | This device has not been given a status by the software.  If this persists, try reconnecting the device, restarting the computer and ensuring the drivers are installed.  |
| <span style="color: red;"        >&#9788;&nbsp;**Flashing Red**</span>           | Starting*    | The device is starting.  Technically, either in a low-battery pre-charge state, or in the 'bootloader'.  If it persists and fails to start properly, then remove the device from circulation.  |

(*) Devices in these states may not appear in the software.


## Operation: Download, Clearing and Recharging

Steps:

1. Fill all free spaces on the docks with devices from the *incoming* bucket.

2. After the devices settle, you expect to see LEDs which are: **white** (downloading) and **cyan** (cleared and recharging).  Downloading of 14 devices in parallel takes approximately 18 minutes, recharging could take a little longer.

3. When devices turn **magenta** they are fully charged and should be removed to a *charged* bucket.

4. If any devices turn **blue**: check if it is a download error (confirm the free space on the laptop, waiting for uploads to complete may free-up capacity), or a communication error (press `F9`, `Y` to reset these devices).

5. Repeat from step 1 while devices remain in the *incoming* bucket.


## Operation: Configuring

Steps:

1. Fill free spaces on one or more docks with devices from the *charged* bucket.

2. After the devices settle, you expect to mainly see LEDs which are **magenta** (charged), and perhaps some **cyan** (recharging).  

3. On a laptop with at least one **magenta** (charged) device, scan the barcode from the letter.  The configuration numbers should appear at the top, and one of the devices (the most charged) will be configured and start to flash.

4. Remove the flashing device from the dock.

5. Scan the device you have removed (this double-checks the device is the configured one).

6. Place the device in the envelope (with the letter).

7. Repeat from step 3, or step 1 if you need more devices.



## Project Settings

This section is only relevant when initially configuring a project.


### Configuration File

The initial working directory is `%USERPROFILE%\Documents\AX3`.  The configuration file `config.ini` is loaded from that directory.  If the working directory is changed (`workingdirectory=`), then any `config.ini` in that directory is also loaded. 

This is an example `config.ini` showing the available options (lines begining `#` are commented-out and therefore inactive):

```ini
# Working directory (default is "%USERPROFILE%\Documents\AX3")
#workingdirectory=C:\AX3

# Software title
title=Software Title
welcome=Welcome banner text (prefix ! for red, * for green)

# Detailed log window open by defalt
log=true

# Require device ids to be scanned after configuration when disconnected
scandevices=false

# File upload/synchronizer (to AWS)
aws_region=REGION
aws_bucket=BUCKET
aws_key=KEY
aws_secret=SECRET

# Back-end configure/download notification API
api_url=https://example.com
pre_shared_key=API_KEY

# Automatically delete downloaded files when they have been successfully uploaded
delete=false
```

### Command-line configuration

Note that command-line options cannot be passed to the version installed from the setup tool as it is a "ClickOnce" installation -- use the .ZIP version instead. 
For use to configure devices launched from another application, the deployment tool supports command-line options:

* `--config <config-code>`
  * Where `<config-code>` is a configuration code as specified below
  * Waits until one cleared and charged device has been configured and disconnected, then automatically exits
    * The program exit code will be set to the programmed device id.
  * The program exit code is `0` if the program is exited before successfully configuring a device, or if the software is already running.
* `--battery <percentage>` to change the default minimum battery percentage for a device (default 82%).


### Configuration codes

The configuration codes are typically represented in a machine-readable barcode or command-line option and consist of numbers separated by a letter prefix signifying the attribute being configured:

* `s` - (default initial attribute) session identifier (up to 9 numeric digits, default 0)
* `b` - required beginning/start time of the recording, one of the formats: `DD` (next occurrance of day-of-month including today; at midnight), `MMDD` (next occurance of month/day including today; at midnight), `MMDDhh` (next occurance of month/day including today; at the specified hour), `YYMMDDhh` (year is `20YY`; at the specified hour), `YYMMDDhhmm`, `YYMMDDhhmmss` (`ss` specified second), `YYYYMMDDhhmmss` (`YYYY` fully-specified year).
* `d` - duration in hours (default 24 * 7 = 168; 7 days)
* `r` - rate (default 100 Hz)
* `g` - range (default +/- 8g)

<!--
*  where config string contains one or more of of the configuration parts concatenated together:
  * `b` to set the recording start time in the format `YYYYMMDDhhmmss`, e.g. `b20180217091500` (required). Also accepts the formats `YYMMDDhhmmss`, `YYMMDDhhmm` (Y=+2000, ss=00), `YYMMDDhh` (mm=00), `MMDDhh` (Y=current/next), `MMDD` (hh=00), `DD` (M=current/next)
  * `d` to set the recording duration in hours, e.g. `d168` (default 168 hours)
  * `s` to set the session ID (up to 9 digits), e.g. `s123456789` (default 0)
  * `r` to set the sampling rate, e.g. `r100` (default 100 Hz)
  * `g` to set the sampling range, e.g. `g8` (default +/- 8g)
-->

As an example, the configuration code:

	10042b1802170915

...signifies a session identifier `10042`, a start time of `09:15` on `17-Feb-2018`, a (default) duration of 168 hours (7 days), a (default) rate of 100 Hz, and a (default) range of +/-8g.

