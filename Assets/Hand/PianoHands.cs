using UnityEngine;
using System.Collections;
using System.Linq;

public class PianoHands : MonoBehaviour
{
    private const int NoteLeftRightThreshold = 35;
    private float BlackKeyOffsetX = 0.07f;

    public PlayPiano player;
    public HandMovement handLeft;
    public HandMovement handRight;

    public bool useRightHandOnly = false;

    private float initHandX;

    private Fingering fingering;
    private int[] optimalFingers;
    private int[] upcomingNoteBuffer = new int[5];

    private static int handSwitches = 0;

    void Awake()
    {
        fingering = new Fingering(player);
    }

    // Use this for initialization
    void Start()
    {
        player.OnNotePress += OnNotePress;
        player.OnNoteRelease += OnNoteRelease;
        player.OnStart += OnStartPiece;

        initHandX = handLeft.transform.position.x;

        if (useRightHandOnly)
            handLeft.TargetPositon += new Vector3(0, 0, 2.0f);
        
    }

    private void OnStartPiece()
    {
        // Pre-calculate fingering
        optimalFingers = fingering.GetOptimalFingersDP(player.GetAllNotes());
    }

    private void OnNotePress(int note)
    {
        HandMovement hand = note >= NoteLeftRightThreshold ? handRight : handLeft;
        OnNotePressWithHand(note, hand);
    }

    private void OnNotePressWithHand(int note, HandMovement hand)
    {
        if (useRightHandOnly)
            hand = handRight;

        HandMovement otherHand = hand == handLeft ? handRight : handLeft;

        int mult = hand == handRight ? 1 : -1;
        int handBase = hand == handRight ? 0 : 4; // Which finger to start on 

        // Get White key positions
        int whiteNote = player.GetKeyPosByNoteIndex(note);
        int whiteBaseNote = player.GetKeyPosByNoteIndex(hand.CurrentBaseNote);

        // Is note in current reach of the hand?
        if (whiteNote >= whiteBaseNote && whiteNote <= whiteBaseNote + 7)
        {
            // If yes, determine the finger
            int finger;
            int lowerNote = hand.LastPlayedNote;
            int upperNote = note;
            if (note < hand.LastPlayedNote)
            {
                lowerNote = note;
                upperNote = hand.LastPlayedNote;
            }
            int interval = upperNote - lowerNote;

            /*if (interval > 0 && interval <= 12)
            {
                finger = fingering.GetOptimalFinger(hand.CurrentFinger + 1, lowerNote, upperNote, note < hand.LastPlayedNote) - 1;
            }
            else
            {
                // Repeated note -> stay on same finger
                finger = hand.CurrentFinger;
            }

            finger = whiteNote - whiteBaseNote;*/

            finger = optimalFingers[player.CurrentNoteIndex] - 1;

            // Crossover
            bool crossover = false;
            int crossoverDownFinger = -1;
            int nextNote = -1;
            if ((nextNote = player.GetUpcomingNote()) != -1)
            {
                int lowerFinger = finger;
                int upperFinger = optimalFingers[player.CurrentNoteIndex + 1] - 1;
                if (nextNote < note)
                {
                    // Next up a decreasing interval
                    lowerFinger = upperFinger;
                    upperFinger = finger;
                    crossoverDownFinger = lowerFinger;
                }
                if ((lowerFinger == 1 || lowerFinger == 2 || lowerFinger == 3) && upperFinger == 0)
                {
                    // Crossover with index, middle or ring finger
                    crossover = true;
                }
            }

            // Check if it requires a greater hand span
            if (finger > 4)
            {
                // Use full octave span, but use different finger depending on actual note
                if (finger == 5) finger = 3;
                else if (finger == 6) finger = 4;
                else if (finger == 7) finger = 4;

                // Set hand to span an entire octave and move it accordingly
                hand.SetOctaveSpan(1.0f);
                int octaveMovement = hand == handLeft ? 3 : 4;
                float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, octaveMovement).z;
                hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                //hand.CurrentBaseNote = note; // Leave at old base note ??
            }
            else
            {
                if (interval > 2)
                {
                    // Determine unusual finger spans
                    float octaveSpan = 0.0f;
                    int fingerDiff = Mathf.Abs(finger - hand.CurrentFinger); // Difference between the two current fingers
                    if (interval <= 4)
                    {
                        if (fingerDiff == 1)
                        {
                            octaveSpan = 0.5f;
                        }
                        else
                        {
                            octaveSpan = 0.0f;
                            //hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, 0);
                        }
                    }
                    else // if (interval <= 7)
                    {
                        if (fingerDiff <= 2)
                        {
                            octaveSpan = 1.0f;
                        }
                        else if (fingerDiff == 3)
                        {
                            octaveSpan = 0.5f;
                        }
                        else
                        {
                            octaveSpan = 0.0f;
                            //hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -4);
                        }
                    }


                    hand.SetOctaveSpan(octaveSpan);
                    int octaveMovement = hand == handLeft ? 3 : 4;
                    float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, octaveMovement).z;
                    hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                }
                else
                {
                    if (finger == 0)
                        hand.CurrentBaseNote = note;
                    else if (finger == 1)
                        hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -1);
                    else if (finger == 2)
                        hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -2);
                    else if (finger == 3)
                        hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -3);
                    if (finger == 4)
                        hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -4);

                    // Reset hand span
                    float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, 2).z;
                    hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                    hand.SetOctaveSpan(0.0f);
                }
            }

            int actualFinger = Mathf.Abs(handBase - finger);
            hand.FingerDown(actualFinger, crossover, crossoverDownFinger);
            hand.LastPlayedNote = note;
            hand.CurrentFingerNotes[actualFinger] = note;
            
        }
        else
        {
            // If not, move hand to the according position

            int finger = optimalFingers[player.CurrentNoteIndex] - 1;

            // Check first, if hand can be moved -> otherwise try to use the other hand
            int interval = Mathf.Abs(note - hand.LastPlayedNote);
            if (!useRightHandOnly && handSwitches < 2 && hand.IsFingerOccupied(finger) && !otherHand.IsOccupied() && interval > 12)
            {
                handSwitches++;
                OnNotePressWithHand(note, otherHand);
                return;
            }
            else
            {
                handSwitches = 0;
            }
            
            if (finger == 0)
                hand.CurrentBaseNote = note;
            else if (finger == 1)
                hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -1);
            else if (finger == 2)
                hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -2);
            else if (finger == 3)
                hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -3);
            if (finger == 4)
                hand.CurrentBaseNote = player.GetWhiteNoteWithOffset(note, -4);

            // Reset hand span
            float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, 2).z;
            hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
            hand.SetOctaveSpan(0.0f);

            hand.FingerDown(finger);
            hand.LastPlayedNote = note;
            hand.CurrentFingerNotes[finger] = note;

        }

        // Handle Black Keys
        if (player.IsBlackKey(note))
        {
            var pos = hand.TargetPositon;
            pos.x = initHandX + BlackKeyOffsetX;
            hand.TargetPositon = pos;
        }
        else
        {
            var pos = hand.TargetPositon;
            pos.x = initHandX;
            hand.TargetPositon = pos;
        }
    }

    private void OnNoteRelease(int note)
    {
        HandMovement hand = note >= NoteLeftRightThreshold ? handRight : handLeft;

        if (useRightHandOnly)
            hand = handRight;

        //int finger = (note - hand.CurrentBaseNote) + 2;
        int finger = hand.CurrentFingerNotes.IndexOf(note);
        if (finger == -1)
            return;

        hand.CurrentFingerNotes[finger] = -1;
        hand.FingerUp(finger);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
