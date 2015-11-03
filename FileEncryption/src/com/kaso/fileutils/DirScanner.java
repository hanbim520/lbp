package com.kaso.fileutils;
//�˰��µĹ�������ɨ���ļ����µ��ļ�,DirScanner��ɨ������FileParser��ɨ������Ҫ�õ��Ľӿ�

import java.io.*;

public class DirScanner {
	public static void scanning(File baseDir, FileParser fp) throws RuntimeException {
		if(!baseDir.isDirectory()) {
			throw new RuntimeException("不是目录");
		}
        fp.start(baseDir);
        try {
            scanning(baseDir, baseDir, fp);
        } catch(FileParserException e) {
            e.printStackTrace();
        }
        fp.end(baseDir);		
	}
	
    private static void scanning(File baseDir, File file, FileParser fp) throws FileParserException {
        if(file.isDirectory()) {
            fp.startDir(baseDir, file);
            File[] files = file.listFiles();
            if(files != null) {
                for(File f : files) {
                    scanning(baseDir, f, fp);
                }
            }
        } else {
            fp.startFile(baseDir, file);
        }
    }
}