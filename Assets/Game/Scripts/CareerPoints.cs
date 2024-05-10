using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CareerPoints : MonoBehaviour
{
    public int points;
    public int lostPoints;
    public List<string> unlocked;
    public TextMeshProUGUI pointsText;
    //public TextMeshProUGUI lostPointsText;

    void Start()
    {
        points = 0;
        lostPoints = 0;
        unlocked = new List<string>();
    }

    //qd completar a missão 
    public void CompleteMission(int earnedPoints)
    {
        points += earnedPoints;
        CheckUnlocked();
        UpdatePointsText();
    }

    //qd o jogador perde pontos
    public void HitObjects(int lostPoints)
    {
        points -= lostPoints;
        this.lostPoints += lostPoints;
        if (points <= 0)
        {
            Debug.Log("Game Over");
        }
        UpdatePointsText();
    }

    //Validar o desbloqueio
    public void CheckUnlocked()
    {
        Dictionary<int, string> pieces = new Dictionary<int, string>()
        {
            {10000, "Piece 1"},
            {20000, "Piece 2"},
            {30000, "Piece 3"}
        };

        foreach (KeyValuePair<int, string> piece in pieces)
        {
            if (points >= piece.Key && !unlocked.Contains(piece.Value))
            {
                unlocked.Add(piece.Value);
            }
        }
    }

    public void UpdatePointsText()
    {
        pointsText.text = "Points: " + points;
        //melhorar
    }

    //Pontos perdidos aparecerem talvez?

    //Dev n esquecer de remover o debug
    public void ShowPoints()
    {
        Debug.Log("Points: " + points);
    }

    public void ShowLostPoints()
    {
        Debug.Log("Lost Points: " + lostPoints);
    }

    public void ShowUnlocked()
    {
        Debug.Log("Unlocked: " + string.Join(", ", unlocked));
    }
}
