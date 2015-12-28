#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <sys/un.h>
#include <sys/ioctl.h>
#include <sys/socket.h>
#include <linux/types.h>
#include <linux/netlink.h>
#include <errno.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <netinet/in.h>

#include <dirent.h>
#include <unistd.h>

#include <sys/stat.h>
#include <sys/types.h>
#include <ftw.h>
#include <fcntl.h>
#include <limits.h>
#include <dirent.h>

#define UEVENT_BUFFER_SIZE 2048
#define ROOT_DIR "/media/root"

#define BUFSIZE 1024
#define PERMS 0666
#define DUMMY 0

typedef enum { false, true }bool;

int cpfile(char *source_file,char *target_file)
{
  int source,target,num;
  char iobuffer[BUFSIZE];
  if((source=open(source_file,O_RDONLY,DUMMY))==-1)
     {
      printf("Source file open error!\n");
      return 1;
     }
  if((target=open(target_file,O_WRONLY|O_CREAT,PERMS))==-1)
    {
      printf("Target file open error!\n");
      return 2;
     }
  while((num=read(source,iobuffer,BUFSIZE))>0)
     if(write(target,iobuffer,num)!=num)
       {
         printf("Target file write error!\n");
         return 3;
       }
   close(source);
   close(target);
   return 0;
}


int cpdir(char *source_dir,char *target_dir){

     DIR *source=NULL;
     DIR *target=NULL;
     struct dirent *ent=NULL;
     char  name1[100],name2[100];

     source=opendir(source_dir);
     mkdir(target_dir,S_IRWXU|S_IRGRP|S_IXGRP|S_IROTH);
     target=opendir(target_dir);
     if(source!=NULL&&target!=NULL)
        {
          while((ent=readdir(source))!= NULL)
          {
             if( strcmp(ent->d_name,"..")!=0 && strcmp(ent->d_name,".")!=0)
                {
                   strcpy(name1,"\0");
                   strcat(name1,source_dir);
                   strcat(name1,"/");
                   strcat(name1,ent->d_name);
                   strcpy(name2,"\0");
                   strcat(name2,target_dir);
                   strcat(name2,"/");
                   strcat(name2,ent->d_name);
                   if(ent->d_type==4)
                       cpdir(name1,name2);
                   if(ent->d_type==8)
                       cpfile(name1,name2);

                }
           }
         closedir(source);
         closedir(target);
       }
 return 0;
}

int readFileList(char *basePath, bool findUpdate)
{
    DIR *dir;
    struct dirent *ptr;

    if ((dir=opendir(basePath)) == NULL)
    {
        perror("Open dir error...");
        printf("%s\n", basePath);
        exit(1);
    }

    while ((ptr=readdir(dir)) != NULL)
    {
        if(strcmp(ptr->d_name, ".")==0 || strcmp(ptr->d_name, "..")==0)    ///current dir OR parrent dir
            continue;
        else if(ptr->d_type == 8)    ///file
            printf("d_name:%s/%s\n", basePath, ptr->d_name);
        else if(ptr->d_type == 10)    ///link file
            printf("d_name:%s/%s\n", basePath, ptr->d_name);
        else if(ptr->d_type == 4)    ///dir
        {
            // find u disk
            if (!findUpdate)
            {
                char path[256];
                memset(path, 0, strlen(path));
                strcat(path, "/media/root/");
                strcat(path, ptr->d_name);
                readFileList(path, true);
            }
            // copy update dir
            else if (findUpdate && strcmp(ptr->d_name, "update") == 0)
            {
                char path[256];
                memset(path, 0, strlen(path));
                strcat(path, basePath);
                strcat(path, "/update");
                cpdir(path, "/home/update");
                break;
            }
        }
    }
    closedir(dir);
    return 1;
}

static int init_hotplug_sock()
{
    const int buffersize = 1024;
    int ret;

    struct sockaddr_nl snl;
    bzero(&snl, sizeof(struct sockaddr_nl));
    snl.nl_family = AF_NETLINK;
    snl.nl_pid = getpid();
    snl.nl_groups = 1;

    int s = socket(PF_NETLINK, SOCK_DGRAM, NETLINK_KOBJECT_UEVENT);
    if (s == -1)
    {
        perror("socket");
        return -1;
    }
    setsockopt(s, SOL_SOCKET, SO_RCVBUF, &buffersize, sizeof(buffersize));

    ret = bind(s, (struct sockaddr *)&snl, sizeof(struct sockaddr_nl));
    if (ret < 0)
    {
        perror("bind");
        close(s);
        return -1;
    }

    return s;
}

// return: numbers of usb
int getUsbNum(char *basePath)
{
    DIR *dir;
    struct dirent *ptr;
    int num = 0;

    if ((dir=opendir(basePath)) == NULL)
    {
        perror("Open dir error...");
        exit(1);
    }

    while ((ptr=readdir(dir)) != NULL)
    {
        if(strcmp(ptr->d_name, ".")==0 || strcmp(ptr->d_name, "..")==0)    ///current dir OR parrent dir
            continue;
        else if(ptr->d_type == 8)    ///file
            printf("d_name:%s/%s\n", basePath, ptr->d_name);
        else if(ptr->d_type == 10)    ///link file
            printf("d_name:%s/%s\n", basePath, ptr->d_name);
        else if(ptr->d_type == 4)    ///dir
        {
            ++num;
        }
    }
    closedir(dir);
    return num;
}

int main(int argc, char* argv[])
{
//    int hotplug_sock = init_hotplug_sock();
//    while(1)
//    {
//        /* Netlink message buffer */
//        char buf[UEVENT_BUFFER_SIZE * 2] = {0};
//        recv(hotplug_sock, &buf, sizeof(buf), 0);
//        printf("%s\n", buf);
//
//        if(!memcmp(buf, "add@", 4) && !memcmp(&buf[strlen(buf) - 4], "/sdb", 4))
//        {
//            sleep(2);
//            readFileList(ROOT_DIR, false);
//        }
//    }

    if (getUsbNum(ROOT_DIR) > 0)
    {
        // scan u disk for updating
        readFileList(ROOT_DIR, false);
    }
    else
    {
        // open game
        system("/usr/games/gnome-mines");
    }
    return 0;
}
