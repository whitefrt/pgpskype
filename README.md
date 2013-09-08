pgpSkype
========

* pgpSkype - encrypted and secure skype conversations.
* copyright (c) 2013 white_frt
* public domain
	
brief:
------
* on each run the tool automatically generates a set of newly disposable public/private PGP keys with a randomly generated secret phrase.
* when starting a conversation, the public key is automatically sent to the other party (which follows by automatically sending his own disposable public key).
* from that moment on, every message is automatically encrypted and decrypted with the PGP keys - conversations are now encrypted and secure.

important:
----------
* ONLY messages sent using pgpSkype's conversation window will be encrypted, messages sent from the regular skype window will be sent unencrypted.
* the other party MUST use pgpSkype.
* NOTHING is saved locally: the keys and secret phrase are randomly generated on each run, there is (intentionally) no way to view conversation history.

this was quickly hacked together as a personal tool, with the intention of being as automatic and barebones as possible - feel free to modify/add features.

usage:
------
* run pgpSkype.exe,
	a small window with your contacts will open - double click a contact to initiate a conversation - send messages only using pgpSkype's conversation window.
	
random notes:
-------------	
* requires Skype4COM.dll (automatically installed with Skype)
* using the bouncycastle library  

further ideas:
* support group conversations
* better UI
* port to other platforms

changelog:
----------
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