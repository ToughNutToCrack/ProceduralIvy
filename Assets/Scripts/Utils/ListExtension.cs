using System.Collections.Generic;
using System.Linq;

public static class ListExtension {

    public static List<T> join<T>(this List<T> first, List<T> second) {
        if (first == null) {
            return second;
        }
        if (second == null) {
            return first;
        }
        return first.Concat(second).ToList();
    }

}