/**************************************************************************************************
* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ *
                               uart-linux.c

    Develop Team     :    7fane Team
    Team  Leader     :    He YiJun  (storysnail<at>gmail.com QQ：363559089)
    Main Programmer  :    He YiJun
    Program comments :    Ling Ying
    License          :    7fane Team  License 1.0
    Last Update      :    2013-03-25
* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ *
  功能说明:
* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ *
  更    新:
* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ *
  已知问题:
* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ *
**************************************************************************************************/

#include <termios.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <sys/select.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "uart-linux.h"

/**************************************************************************************************
函数名称：Com_Open()
函数功能：打开串口
函数说明：无
入口参数：无
出口参数：成功返回串口设备文件描述符,失败返回-1
调用实例：无
**************************************************************************************************/
int Com_Open(void)
{
  int fd = -1;

  fd = open("/dev/ttyS0", O_RDWR | O_NOCTTY | O_NONBLOCK);
  if(fd == -1) {
    perror("open_port: Unable to open /dev/ttyS0");
  }

  if(Com_SetIOBlockFlag(fd,BLOCK_IO) == -1)
    printf("IO set error\n");

  return (fd);
}

/**************************************************************************************************
函数名称：Com_Close()
函数功能：关闭串口
函数说明：无
入口参数：fd:串口设备文件描述符
出口参数：无
调用实例：无
**************************************************************************************************/
int Com_Close(int fd)
{
  if (fd < 0)
    return -1;

  if (close(fd) == -1)
    return -1;

  printf("close uart\n\n");

  return 0;
}

/**************************************************************************************************
函数名称：Com_SetIOBlockFlag()
函数功能：设置IO为阻塞或非阻塞
函数说明：无
入口参数：fd:串口设备文件描述符  value:BLOCK_IO或NONBLOCK_IO
出口参数：失败返回-1,否则返回其它值
调用实例：无
**************************************************************************************************/
int Com_SetIOBlockFlag(int fd,int value)
{
  int oldflags;

  if (fd == -1)
    return -1;

  oldflags = fcntl(fd,F_GETFL,0);
  if(oldflags == -1) {
    printf("get IO flags error\n");
    return -1;
  }

  if(value == BLOCK_IO)
    oldflags &= ~O_NONBLOCK;  //设置成阻塞IO
  else
    oldflags |= O_NONBLOCK;   //设置成非阻塞IO

  return fcntl(fd,F_GETFL,oldflags);
}

/**************************************************************************************************
函数名称：Com_GetInBufSize()
函数功能：得到串口输入队列中的字节数
函数说明：无
入口参数：fd:串口设备文件描述符  InBufSize:串口输入队列中的字节数会保存在该指针所指向的内存
出口参数：失败返回-1,否则返回0
调用实例：无
**************************************************************************************************/
int Com_GetInQueByteCount(int fd,int *ByteCount)
{
  int bytes = 0;

  if (fd == -1)
    return -1;

  if(ioctl(fd, FIONREAD, &bytes) != -1) {
    *ByteCount = bytes;
    return 0;
  }

  return -1;
}

/**************************************************************************************************
函数名称：Com_Setup()
函数功能：串口设定函数
函数说明：无
入口参数：fd:串口设备文件描述符
           baud:比特率 300、600、1200、2400、4800、9600、19200、38400、57600、115200
           databit:一个字节的数据位个数 5、6、7、8
           stopbit:停止位个数1、2
           parity:奇偶校验 0:无奇偶效验  1:奇效验  2:偶效验
           flow：硬件流控制 0：无流控、 1：软件流控  2：硬件流控
出口参数：失败返回-1,否则返回0
调用实例：无
**************************************************************************************************/
int Com_Setup(int fd,unsigned int baud, int databit, int stopbit, int parity, int flow)
{
  struct termios options;

  if (fd == -1)
    return -1;

  if(tcgetattr(fd, &options) == -1)
    return -1;

      switch (baud) {  //取得比特率
      case 300:
        options.c_cflag =  B300;
        break;
      case 600:
        options.c_cflag =  B600;
        break;
      case 1200:
        options.c_cflag =  B1200;
        break;
      case 2400:
        options.c_cflag =  B2400;
        break;
      case 4800:
        options.c_cflag =  B4800;
        break;
      case 9600:
        options.c_cflag =  B9600;
        break;
      case 19200:
        options.c_cflag =  B19200;
        break;
      case 38400:
        options.c_cflag =  B38400;
        break;
      case 57600:
        options.c_cflag =  B57600;
        break;
      case 115200:
        options.c_cflag =  B115200;
        break;
      default:
        options.c_cflag =  B19200;
        break;
      }

  switch (databit) {  //取得一个字节的数据位个数
  case 5:
    options.c_cflag &= ~CSIZE;
    options.c_cflag |= CS5;
    break;
  case 6:
    options.c_cflag &= ~CSIZE;
    options.c_cflag |= CS6;
    break;
  case 7:
    options.c_cflag &= ~CSIZE;
    options.c_cflag |= CS7;
    break;
  case 8:
    options.c_cflag &= ~CSIZE;
    options.c_cflag |= CS8;
    break;
  default:
    options.c_cflag &= ~CSIZE;
    options.c_cflag |= CS8;
    break;
  }

  switch (parity) {  //取得奇偶校验
  case 0:
    options.c_cflag &= ~PARENB;              // 无奇偶效验
    options.c_iflag &= ~(INPCK  | ISTRIP);   // 禁用输入奇偶效验
    options.c_iflag |= IGNPAR;               // 忽略奇偶效验错误
    break;
  case 1:
    options.c_cflag |= (PARENB | PARODD);    // 启用奇偶效验且设置为奇效验
    options.c_iflag |= (INPCK  | ISTRIP);    // 启用奇偶效验检查并从接收字符串中脱去奇偶校验位
    options.c_iflag &= ~IGNPAR;              // 不忽略奇偶效验错误
    break;
  case 2:
    options.c_cflag |= PARENB;               // 启用奇偶效验
    options.c_cflag &= ~PARODD;              // 设置为偶效验
    options.c_iflag |= (INPCK  | ISTRIP);    // 启用奇偶效验检查并从接收字符串中脱去奇偶校验位
    options.c_iflag &= ~IGNPAR;              // 不忽略奇偶效验错误
    break;
  default:
    options.c_cflag &= ~PARENB;              // 无奇偶效验
    options.c_iflag &= ~(INPCK  | ISTRIP);   // 禁用输入奇偶效验
    options.c_iflag |= IGNPAR;               // 忽略奇偶效验错误

    break;
  }

  switch (stopbit) {  //取得停止位个数
  case 1:
    options.c_cflag &= ~CSTOPB;               // 一个停止位
    break;
  case 2:
    options.c_cflag |= CSTOPB;                // 2个停止位
    break;
  default:
      options.c_cflag &= ~CSTOPB;               // 默认一个停止位
    break;
  }

  switch (flow) {  //取得流控制
  case 0:
    options.c_cflag &= ~CRTSCTS;                // 停用硬件流控制
    options.c_iflag &= ~(IXON | IXOFF | IXANY); // 停用软件流控制
    options.c_cflag |= CLOCAL;                  // 不使用流控制
  case 1:
    options.c_cflag &= ~CRTSCTS;                // 停用硬件流控制
    options.c_cflag &= ~CLOCAL;                 // 使用流控制
    options.c_iflag |= (IXON | IXOFF | IXANY);  // 使用软件流控制
    break;
  case 2:
    options.c_cflag &= ~CLOCAL;                 // 使用流控制
    options.c_iflag &= ~(IXON | IXOFF | IXANY); // 停用软件流控制
    options.c_cflag |= CRTSCTS;                 // 使用硬件流控制
    break;
  default:
    options.c_cflag &= ~CRTSCTS;                // 停用硬件流控制
    options.c_iflag &= ~(IXON | IXOFF | IXANY); // 停用软件流控制
    options.c_cflag |= CLOCAL;                  // 不使用流控制
    break;
  }

  options.c_cflag |= CREAD;                     // 启用接收器
  options.c_iflag |= IGNBRK;                    // 忽略输入行的终止条件
  options.c_oflag = 0;                          // 非加工方式输出
  options.c_lflag = 0;                          // 非加工方式
  //options.c_lflag     &= ~(ICANON | ECHO | ECHOE | ISIG);
  //options.c_oflag     &= ~OPOST;
  //如果串口输入队列没有数据，程序将在read调用处阻塞
  options.c_cc[VMIN]  = 1;
  options.c_cc[VTIME] = 0;

  if(tcsetattr(fd, TCSANOW, &options) == -1)    // 保存配置并立刻生效
    return -1;

  // DTR RTS
//  int status;
//  ioctl(fd, TIOCMGET, &status);
//  status &= ~TIOCM_DTR;
//  status &= ~TIOCM_RTS;
//  ioctl(fd, TIOCMSET, &status);

  //清空串口输入输出队列
  tcflush(fd, TCOFLUSH);
  tcflush(fd, TCIFLUSH);

  return 0;
}

/**************************************************************************************************
函数名称：Com_ChangeBaudrate()
函数功能：设定串口波特率
函数说明：无
入口参数：fd:串口设备文件描述符
           baud:比特率 300、600、1200、2400、4800、9600、19200、38400、57600、115200
出口参数：成功返回0，失败返回-1
调用实例：无
**************************************************************************************************/
int Com_ChangeBaudrate(int fd, unsigned int baud)
{
  struct termios options;
  struct termios old_options;
  unsigned int baudrate = B19200;

  if (fd == -1)
    return -1;

  if(tcgetattr(fd, &old_options) == -1)
    return -1;

  if(tcgetattr(fd, &options) == -1)
    return -1;

  switch (baud) {
  case 300:
    baudrate =  B300;
    break;
  case 600:
    baudrate =  B600;
    break;
  case 1200:
    baudrate =  B1200;
    break;
  case 2400:
    baudrate =  B2400;
    break;
  case 4800:
    baudrate =  B4800;
    break;
  case 9600:
    baudrate =  B9600;
    break;
  case 19200:
    baudrate =  B19200;
    break;
  case 38400:
    baudrate =  B38400;
    break;
  case 57600:
    baudrate =  B57600;
    break;
  case 115200:
    baudrate =  B115200;
    break;
  default:
    baudrate =  B19200;
    break;
  }

  if(cfsetispeed(&options, baudrate) == -1)
    return -1;

  if(cfsetospeed(&options, baudrate) == -1) {
    tcsetattr(fd, TCSANOW, &old_options);
    return -1;
  }

  while(tcdrain(fd) == -1);//tcdrain(fd);保证输出队列中的所有数据都被传送

  //清空串口输入输出队列
  tcflush(fd, TCOFLUSH);
  tcflush(fd, TCIFLUSH);

  if(tcsetattr(fd, TCSANOW, &options) == -1) {
    tcsetattr(fd, TCSANOW, &old_options);
    return -1;
  }

  return 0;
}

/**************************************************************************************************
函数名称：Com_Read()
函数功能：接收数据
函数说明：无
入口参数：fd:串口设备文件描述符
           ReadBuffer:将数据写入ReadBuffer所指向的缓存区,并返回实际读到的字节数
           ReadSize:欲读取的字节数
出口参数：成功返回实际读到的字节数，失败返回-1
调用实例：无
**************************************************************************************************/
ssize_t Com_Read(int fd, unsigned char **data)
{
  ssize_t rCount = 0;  //实际读到的字节数
  ssize_t dwBytesRead = 0;
//  int InQueByteCount = 0;
  int ReadSize = COM_MAX_BUFFER;
  unsigned char *ReadBuffer = (unsigned char*)malloc(ReadSize);

  if (fd < 0) {
    free(ReadBuffer);
//    perror("file description is valid");
    return -1;
  }

  if (ReadBuffer == NULL) {
    free(ReadBuffer);
//    perror("read buf is NULL");
    return -1;
  }

  if(ReadSize > COM_MAX_BUFFER)
    dwBytesRead = COM_MAX_BUFFER;
  else
    dwBytesRead = ReadSize;

  memset(ReadBuffer, 0, dwBytesRead);

//  if(Com_GetInQueByteCount(fd,&InQueByteCount) != -1) {
//    printf("Uart Queue have %d bytes\n",InQueByteCount);
//    dwBytesRead=UART_MIN(dwBytesRead,InQueByteCount);
//  }
//
//  if(!dwBytesRead)
//    return -1;

  rCount = read(fd, ReadBuffer, dwBytesRead);
  if (rCount < 0) {
    free(ReadBuffer);
//    perror("read error\n");
    return -1;
  }

  *data = ReadBuffer;
  return rCount;
}

/**************************************************************************************************
函数名称：Com_Write()
函数功能：发送数据
函数说明：无
入口参数：fd:串口设备文件描述符
           WriteBuffer:将WriteBuffer所指向的缓冲区中的数据写入串口
           WriteSize:欲写入的字节数
出口参数：成功返回实际写入的字节数，失败返回-1
调用实例：无
**************************************************************************************************/
ssize_t Com_Write(int fd, unsigned char *WriteBuffer, ssize_t WriteSize)
{
  ssize_t wCount = 0;  //实际写入的字节数
  ssize_t dwBytesWrite=WriteSize;

  if (fd < 0) {
    perror("file description is valid");
    return -1;
  }

  if((dwBytesWrite > COM_MAX_BUFFER) || (!dwBytesWrite))
    return -1;

  wCount = write(fd, WriteBuffer, dwBytesWrite);
  if (wCount < 0) {
    perror("write errot\n");
    return -1;
  }

  while(tcdrain(fd) == -1);

  return wCount;
}

void FreeArrayPtr(char *ptr)
{
	free(ptr);
}
