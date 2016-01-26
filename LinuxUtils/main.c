#include <stdlib.h>
#include <time.h> /*需引用的头文件*/

int GetRandom(int min, int max)
{
    srand((unsigned)time(NULL));            /*随机种子*/
    return rand() % (max - min + 1) + min;  /*n为X~Y之间的随机数*/
}

