# Deploy

AX3 setup and download tool.

*Deploy* is a graphical front-end for *DeployLib* which headlessly performs all of the work.


## Launchable setup tool

* Configuration command-line option `--config <config-string>` where config string contains one or more of of the configuration parts concatenated together:
  * `b` to set the recording start time in the format `YYYYMMDDhhmmss`, e.g. `b20180217091500` (required). Also accepts the formats `YYMMDDhhmmss`, `YYMMDDhhmm` (Y=+2000, ss=00), `YYMMDDhh` (mm=00), `MMDDhh` (Y=current/next), `MMDD` (hh=00), `DD` (M=current/next)
  * `d` to set the recording duration in hours, e.g. `d168` (default 168 hours)
  * `s` to set the session ID (up to 9 digits), e.g. `s123456789` (default 0)
  * `r` to set the sampling rate, e.g. `r100` (default 100 Hz)
  * `g` to set the sampling range, e.g. `g8` (default +/- 8g)
* Command-line option `--battery <percentage>` for minimum battery to start recording (default 82%).
* The program exit code is `0` if the program is exited before successfully configuring a device.
* When a command-line configuration is specified, the program will automatically exit once a device has been successfully programmed and then disconnected, the exit code will be set to the programmed device identifier.


/ Switch to using OmApi directly
/ Run each device in its own thread
/ Read the current configuration
/ Command queue
/ Start/stop device threads
/ Status calculation
/ LED flash logic
/ Trigger update of battery level
/ LED color based on group
/ Fix 'group' determination and reliably detect change events 
/ Monitor a change of 'group' for each device and use that to trigger actions.
/ Trigger download
/ Trigger clear command (after confirm download size)
/ Trigger configure command (settings, time sync, commit)
/ Instructional flow (scan setup barcode, program and flash, scan device id)
/ Warnings (e.g. device removed before downloaded/recharged)
/ Double-buffer list
/ Check single instance
/ Working directory (default, command line option, explore to)
/ Integrate uploader code
/ Call SyncWatcher after device configuration
/ Status bar (attached devices and uploader status)
/ Settings for uploader from working directory
/ Add device scan option
/ Advanced "clear pending configuration"
/ Copy device ids to clipboard
/ Improve pending/recording device status information
/ Advanced "treat as charged" option 
/ Advanced "download" option (with warnings) for unexpected pending/underway devices.
/ Advanced "clear" option (with warnings) for unexpected pending/underway devices.
/ Create installer
/ Create documentation (markdown)
/ Link to documentation from program
/ Fix panel size
/ Don't flag comms error during download
/ Reset devices with comms errors
/ Change battery update interval once charged or when downloading
/ Fix multiple configs when not needing scan
/ Disallow duplicate scans
/ Show (successfully) configured over device error
/ Disallow expired configurations
/ Warn about already-started configurations
/ Comments in config
/ Add title and banner from config
/ Charged->still show %
/ Shrink log size
/ "Delete" option to delete after upload
/ Fix upload multi-file sync
/ Delete failed downloads (.part files)
/ Display avaiable drive capacity.
/ Check if date is too far in the future
/ Prevent scan when device not yet removed.
/ Actually fixed formatting of unexpected device config.
/ Tweak "configured" text to prompt for scan.
/ Reduce "fully-charged" threshold to 82%
/ Deleted unused 'Status' class
/ Fix for available space (e.g. when an underlying path is on another drive)
/ Don't upload QC test data (but will download to a ".qcdata" file)
/ Add upload failure retry with back-off (and show on interface)
/ Ability to specify working directory in config (which will also load the config in that folder)


## DeployLib

Top-level methods/events:

* DeviceAdded
* DeviceRemoved
* DeviceUpdated
* Download
* DeviceDownloaded(or failure)
* Clear
* DeviceCleared(or failure)
* Configure
* DeviceConfigured(or failure)

### Device

Represents the state of a device.

* Battery level
* Battery state (with hysteresis, charging/charged)
* Configuration progress
* Configuration state:
    * HAS_DATA
	* CONFIGURED_NONE/CONFIGURED_PENDING/CONFIGURED_DURING/CONFIGURED_FINISHED
	* HAS_CAPACITY
* Download progress
* Downloaded flag (from file attribute)
* Overall State:
	* ERROR (a failure has occurred, reconnect?)
	* UNKNOWN (device attached but not yet examined)
	* UNDERWAY (has data or a configuration that has started; and a configuration that has not yet finished and spare capacity)
	* COMPLETE (has a configuration that has finished or has data and no configuration or has no spare capacity)
	* DOWNLOADING (download in progress)
	* DOWNLOADED (download completed)
	* CLEARING (clearing device in progress)
	* CLEARED (has no data and no configuration)
	* CONFIGURING (setting a new configurtaion in progress)
	* CONFIGURED (has no data and a configuration that has not yet started)

* LED:
	* ERROR: blue
	* DOWNLOADING: white
	* CLEARED and charging: red
	* CLEARED and fully charged: magenta
	* (unexpected connection of) CONFIGURED/UNDERWAY: cyan
	* Identify(after successfully configured): flashing cyan/off

* Firmware-defined LED:
	* Initial connection (pre-charge or bootloader): flashing red
	* Connected to power, no data, not fully charged: yellow
	* Connected to power, no data, fully charged: green
	* Connected to USB, not fully charged: pulsing yellow
	* Connected to USB, fully charged: pulsing white


* A device has just been connected:
  * Request the configuration status of the device.
  * Look at the data on the device.
