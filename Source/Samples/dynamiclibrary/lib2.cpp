#include <functional>

int post(std::function<int(int, int)>&& func, int a, int b)
{
	return func(a, b);
}
