pgpSkype
========

* pgpSkype - encrypted and secure skype conversations.
* copyright (c) 2013 white_frt
* public domain
	
brief:
------
* on EACH run the tool automatically generates a set of newly disposable PUBLIC/PRIVATE RSA-AES keys (2048bits) with a randomly generated secret phrase (128 characters).
* when starting a conversation, the PUBLIC key is automatically sent to the other party (which follows by automatically sending his own disposable PUBLIC key).
* from that moment on, every message is automatically encrypted with the target's PUBLIC key and decrypted with the local PRIVATE key - conversations are now encrypted and secure.

important:
----------
* every user has a one-time set of PUBLIC/PRIVATE keys, messages sent to a user must be encrypted with the user's PUBLIC key and can ONLY be decrypted locally by him using his PRIVATE key (which is per-user and ONLY known to the recipient user).
* the PRIVATE keys are never transmitted, they are disposable per-session-per-user keys, every user has a single PRIVATE key which is only kept locally until the tool shuts down.
* there is no way for 3rd parties to decrypt messages without knowing the 2048bits PRIVATE key of the recipient AND guessing the 128 characters randomly generated paraphrase needed to use the private key - this requires hundreds of years to brute force - knowing the PUBLIC key by itself is useless.

notes:
------
* ONLY messages sent using pgpSkype's conversation window will be encrypted, messages sent from the regular skype window will be sent unencrypted.
* the other party MUST use pgpSkype.
* NOTHING is saved locally: the keys and secret phrase are randomly generated on each run, there is (intentionally) no way to view conversation history.

this was quickly hacked together as a personal tool, with the intention of being as automatic and barebones as possible - feel free to modify/add features.

usage:
------
* run pgpSkype.exe
* a small window with your contacts will open
* double click a contact to initiate a conversation - send messages only using pgpSkype's conversation window.
	
random notes:
-------------	
* requires Skype4COM.dll (automatically installed with Skype)
* using the bouncycastle library  

changelog:
----------
* [004]: settings UI
* [004]: XML settings - save and load
* [004]: ability to change conversation font
* [004]: added message timestamps
* [004]: always on top / show in taskbar optional
* [004]: ability to upgrade to newer versions (>1)
* [004]: updated readme
* [003]: close vanilla skype conversation windows when encrypted
* [003]: mark encrypted messages as seen
* [003]: start the skype client if not running
* [003]: proper display of user names [displayname vs handle vs fullname]
* [003]: dynamically attach to the skype client when closed/reopened
* [003]: moved win32 specifics to Win32.cs
* [002]: increased encryption to 2048 bits RSA-AES256 combination (from 1024 bits RSA-CRYPT5) -> increased loading time
* [002]: increased randomized secret phrase to 128 characters (from 32 characters)
* [002]: refactored code
* [002]: cleaned and removed dangling variables
* [002]: async auto update notification
* [002]: window flashes only twice when a message is received

further ideas:
* support group conversations
* better UI
* port to other platforms
