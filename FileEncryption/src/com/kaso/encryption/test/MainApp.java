package com.kaso.encryption.test;

import javax.swing.JFrame;

import com.kaso.encryption.view.EncryptionFrame;

public class MainApp {
	public static void main(String[] args) {
		EncryptionFrame ef = new EncryptionFrame();
		ef.pack();
		ef.setVisible(true);
		ef.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		
	}	
}
