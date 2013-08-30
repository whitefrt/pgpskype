pgpSkype
========

* pgpSkype - encrypted and secure skype conversations.
* copyright (c) 2013 white_frt
* public domain
	

on every run the tool automatically generates a set of newly disposable public/private PGP keys using a randomly generated secret phrase.
when starting a conversation using pgpSkype, the public key is automatically sent to the other party (which follows by automatically sending his own disposable public key).
from that moment on, every message with that user is automatically encrypted and decrypted using the key sets - completly encrypted and secure.

important:
----------
* ONLY messages sent using pgpSkype's conversation window will be encrypted, messages sent from the skype window will be sent unencrypted.
* the other party MUST use pgpSkype.
* NOTHING is saved locally: the keys and secret phrase are randomly generated on each run, there is (intentionally) no way to view conversation history.

this was quickly hacked together as a personal tool, with the intention of being as automatic and barebones as possible - feel free to modify/add features.

usage: (skype needs to be running and online)
	run pgpSkype.exe
		a small window with your contacts will open - double click a contact to initiate a conversation - send messages only using pgpSkype's conversation window.
	
	
* requires Skype4COM.dll (automatically installed with Skype)
* using the bouncycastle library  

further ideas:
* support group conversations
* better UI
* port to other platforms