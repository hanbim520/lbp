#include <stdlib.h>
#include <time.h> /*需引用的头文件*/

void SetSeed()
{
    srand((unsigned)time(NULL));            /*随机种子*/
}

void Seed(int seed)
{
	srand(seed);
}

int GetRandom(int min, int max)
{
    return rand() % (max - min) + min;  /*n为X~Y之间的随机数*/
}

