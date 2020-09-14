# SleepOnLan
Small Project that can receive WOL (wake on lan) packages verify them and then perform an action of choice (sleep for now)

## What is WOL? (Wake On LAN)
WOL is an ethernet tool that allows you to turn on any LAN network device via its physical address (MAC).

## What does the program do?
Sleep On Lan subscribes to a UDP port (9, but configurable) on your pc. 
This is where WOL packages are sent to by default, when the program receives a package, it verifys that it was an official WOL package.
Once this has been verified it throws an event that then checks the users idle time.
If you have not moved the mouse or touched the keyboard for 5 seconds before sending the WOL package the program activates.
Once activated it waits for 15 seconds before checking if the user has stopped being idle, if this is the case the program cancels the sleep activity.

## How to use
The first release is not really usefull the program starts in a console and needs to be kept running for this to work.

1. download the latest release and extract it somewhere.
2. open the config file and replace the existing (fake) mac address with the address from your pc (this is to filter incoming WOL messages, incase someone tries to prank you)
3. after changing the mac run the only .exe in the folder, the program tells you that its running, try to send a WOL package to your pc.
4. if the pc has not been touched for 5 seconds upon receiving the message, it waits for another 15 seconds before turning the pc into sleep mode.

## Configurables
The program has a few configurable options
1. the MAC address, default is a fake mac address as a placeholder.
2. the WOL port to use, 9 is default here.
3. the initial idle time, 5s is default here.
4. the final idle time (time after intitial idle time), 15s is default here.
