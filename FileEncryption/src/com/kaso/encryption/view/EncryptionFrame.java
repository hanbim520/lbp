package com.kaso.encryption.view;

import java.awt.GridLayout;
import java.awt.HeadlessException;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;

import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JPasswordField;
import javax.swing.JTextField;

import com.kaso.encryption.service.Encryption;

public class EncryptionFrame extends JFrame {
	private JTextField filePath = new JTextField(15); // �ļ�·���ı���
	private JPasswordField pwd = new JPasswordField(10); // �����
	private JButton chooseBtn = new JButton("选择目录"); // ѡ���ļ���ť
	private JButton confirmBtn = new JButton("加密/解密"); // ȷ�ϰ�ť
	private JCheckBox decrypt = new JCheckBox("解密");
	private JLabel fileLabel = new JLabel("选择目录:"); // ѡ���ļ���ǩ
	private JLabel pwdLabel = new JLabel("输入密码:"); // �����ǩ
	private JPanel filePanel = new JPanel(); // ѡ���ļ����
	private JPanel pwdPanel = new JPanel(); // �������
	private JFileChooser jfc = new JFileChooser(new File("."));
	private File file;

	public EncryptionFrame() throws HeadlessException {
		this.init();
		this.addComponent();
		this.addListener();
	}

	private void init() {
		this.setLayout(new GridLayout(2, 1));
		this.setSize(500, 140);
		this.setLocation(400, 300);
		this.setTitle("文件加密/解密器");
	}

	private void addComponent() {
		filePanel.add(fileLabel);
		filePanel.add(filePath);
		filePanel.add(chooseBtn);

		pwdPanel.add(pwdLabel);
		pwdPanel.add(pwd);
		pwdPanel.add(decrypt);
		pwdPanel.add(confirmBtn);

		this.add(filePanel);
		this.add(pwdPanel);

		jfc.setFileSelectionMode(JFileChooser.FILES_AND_DIRECTORIES); // ���ÿ�ѡ���ļ�����
	}

	private void addListener() {
		chooseBtn.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				int result = jfc.showOpenDialog(EncryptionFrame.this); // ���ļ�ѡ����
				if (result == JFileChooser.APPROVE_OPTION) { // ȷ������
					file = jfc.getSelectedFile();
					if (!file.isDirectory()) {
						JOptionPane
								.showMessageDialog(EncryptionFrame.this,
										"请选择目录!", "警告",
										JOptionPane.INFORMATION_MESSAGE);
						return;
					}
					filePath.setText(file.getAbsolutePath()); // ·�������ı���
				}
			}
		});

		confirmBtn.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				if (filePath.getText().equals("")) { // �ж��Ƿ�ѡ���ļ�
					JOptionPane.showMessageDialog(EncryptionFrame.this,
							"请选择文件!", "提示", JOptionPane.INFORMATION_MESSAGE);
					return;
				}
				if (decrypt.isSelected()) { // �жϽ��ܿ��Ƿ�ѡ��
					Encryption.decrypt(EncryptionFrame.this.file, pwd
							.getPassword()); // ���ý��ܷ���
					JOptionPane.showMessageDialog(EncryptionFrame.this,
							"解密完成!", "解密", JOptionPane.INFORMATION_MESSAGE);

				} else {
					Encryption.encrypt(EncryptionFrame.this.file, pwd
							.getPassword());// ���ü��ܷ���
					JOptionPane.showMessageDialog(EncryptionFrame.this,
							"加密完成!", "加密", JOptionPane.INFORMATION_MESSAGE);
				}
			}
		});
	}
}
