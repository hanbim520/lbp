package com.kaso.encryption.service;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import com.kaso.fileutils.FileParser;
import com.kaso.fileutils.FileParserException;

public class EncryptionParser implements FileParser {
	private int index = 0;
	private char[] key;

	public EncryptionParser(char[] key) {
		this.key = key;
	}

	public void end(File baseDir) {
	}

	public void start(File baseDir) {
	}

	public void startDir(File baseDir, File dir) throws FileParserException {
	}

	public void startFile(File baseDir, File file) throws FileParserException {
		InputStream in = null;
		OutputStream out = null;
		try {
			in = new FileInputStream(file);
			out = new FileOutputStream(file.getAbsolutePath() + ".encry");
			byte[] buff = new byte[1024]; // ������
			int n = -1;
			while ((n = in.read(buff)) != -1) {
				for (int i = 0; i < buff.length; i++) {// ��������м���
					buff[i] += key[getIndex()];
				}
				out.write(buff, 0, n);
				out.flush();
			}
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		} finally {
			try {
				in.close();
				out.close();
				file.delete(); // ɾ��Դ�ļ�
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}

	private int getIndex() { // ���key�±�ѭ������
		index = index++ % key.length;
		return index;
	}
}
