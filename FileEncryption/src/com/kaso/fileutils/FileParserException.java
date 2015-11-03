package com.kaso.fileutils;

import java.io.*;

public class FileParserException extends IOException {
	public FileParserException() {
		super();
	}
	
	public FileParserException(Throwable t) {
		super(t);
	}
	
	public FileParserException(String s) {
		super(s);
	}
	
	public FileParserException(String s, Throwable t) {
		super(s, t);
	}
}