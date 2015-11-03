package com.kaso.fileutils;

import java.io.*;

public interface FileParser {
	void start(File baseDir); //扫描开始时的动作

	void end(File baseDir); //扫描结束时的动作

	void startDir(File baseDir, File dir)
			throws FileParserException; //扫描到目录时的动作

	void startFile(File baseDir, File file)
			throws FileParserException; //扫描到文件时的动作
}