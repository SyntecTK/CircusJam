using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static int CalculateRowScore(Board board, int row)
    {
        Card a = board.GetCard(row, 0);
        Card b = board.GetCard(row, 1);
        Card c = board.GetCard(row, 2);

        int av = a?.value ?? 0;
        int bv = b?.value ?? 0;
        int cv = c?.value ?? 0;

        if(av == bv && bv == cv)
            return (av*3)+(bv*3)+(cv*3);

        if(av == bv)
            return (av*2)+(bv*2)+cv;

        if(av == cv)
            return (av*2)+bv+(cv*2);

        if(bv == cv)
            return av+(bv*2)+(cv*2);

        return av + bv + cv;
    }
}
