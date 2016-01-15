/**************************************************************************************************
* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ *
                               uart-linux.h

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

#ifndef __UART_LINUX_H__
#define __UART_LINUX_H__

#define UART_MIN(A,B)  ((A) < (B) ? (A):(B))

#define COM_MAX_BUFFER   512  //串口数据缓存的最大字节数

#define BLOCK_IO    1
#define NONBLOCK_IO 1

extern int Com_Open(void);
extern int Com_Close(int fd);
extern int Com_SetIOBlockFlag(int fd,int value);
extern int Com_GetInQueByteCount(int fd,int *ByteCount);
extern int Com_Setup(int fd,unsigned int baud, int databit, int stopbit, int parity, int flow);
extern int Com_ChangeBaudrate(int fd, unsigned int baud);
extern ssize_t Com_Read(int fd, unsigned char **ReadBuffer);
extern ssize_t Com_Write(int fd, unsigned char *WriteBuffer, ssize_t WriteSize);
extern void FreeArrayPtr(char *ptr);

#endif
