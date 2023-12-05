#include <stdio.h>
#include <stdbool.h>

void printInt(int x)
{
	printf("%d\n", x);
}

void printReal(double x)
{
	printf("%f\n", x);
}

void printBool(bool x)
{
	printf("%s\n", x ? "true" : "false");
}
