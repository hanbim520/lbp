package com.kaso.fileutils;

import java.io.*;

public interface FileParser {
	void start(File baseDir); //ɨ�迪ʼʱ�Ķ���

	void end(File baseDir); //ɨ�����ʱ�Ķ���

	void startDir(File baseDir, File dir)
			throws FileParserException; //ɨ�赽Ŀ¼ʱ�Ķ���

	void startFile(File baseDir, File file)
			throws FileParserException; //ɨ�赽�ļ�ʱ�Ķ���
}