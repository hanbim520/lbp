package com.kaso.encryption.service;

import java.io.File;

import com.kaso.fileutils.DirScanner;

public class Encryption {
	
	public static void encrypt(File file, char[] key) {
		DirScanner.scanning(file, new EncryptionParser(key));
	}

	public static void decrypt(File file, char[] key) {
		DirScanner.scanning(file, new DecryptionParser(key));
	}
}
