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

            if (interval > 0 && interval <= 12)
            {
                finger = fingering.GetOptimalFinger(hand.CurrentFinger + 1, lowerNote, upperNote, note < hand.LastPlayedNote) - 1;
            }
            else
            {
                // Repeated note -> stay on same finger
                finger = hand.CurrentFinger;
            }

            //finger = whiteNote - whiteBaseNote;

            // Check if a cross-over should be performed
            bool crossover = false;
            player.GetUpcomingNoteBuffer(upcomingNoteBuffer);
            if (finger == 2 && upcomingNoteBuffer[0] > note && upcomingNoteBuffer[1] > upcomingNoteBuffer[0])
            {
                //finger = 0;
                //hand.CurrentBaseNote = note;
                crossover = true;
            }

            //finger = optimalFingers[player.CurrentNoteIndex];

            // Crossover
            /*bool crossover = false;
            if (finger == 2 && player.CurrentNoteIndex + 1 < optimalFingers.Length && optimalFingers[player.CurrentNoteIndex + 1] == 0)
            {
                // Crossover with middle finger
                crossover = true;
            }*/

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
                    hand.SetOctaveSpan(1.0f);
                    int octaveMovement = hand == handLeft ? 3 : 4;
                    float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, octaveMovement).z;
                    hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                }
                else
                {
                    if (finger == 0)
                        hand.CurrentBaseNote = note;
                    else if (finger == 1)
                        hand.CurrentBaseNote = note - 2;
                    else if (finger == 2)
                        hand.CurrentBaseNote = note - 4;
                    else if (finger == 3)
                        hand.CurrentBaseNote = note - 5;
                    if (finger == 4)
                        hand.CurrentBaseNote = note - 7;

                    // Reset hand span
                    float zPos = player.GetNoteWorldPosition(hand.CurrentBaseNote, 2).z;
                    hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                    hand.SetOctaveSpan(0.0f);
                }
            }

            int actualFinger = Mathf.Abs(handBase - finger);
            hand.FingerDown(actualFinger, crossover);
            hand.LastPlayedNote = note;
            hand.CurrentFingerNotes[actualFinger] = note;
            
        }
        else
        {
            // If not, move hand to the according position

            // Check first, if hand can be moved -> otherwise try to use the other hand
            if (!useRightHandOnly && handSwitches < 2 && hand.IsOccupied() && !otherHand.IsOccupied())
            {
                handSwitches++;
                OnNotePressWithHand(note, otherHand);
                return;
            }
            else
            {
                handSwitches = 0;
            }

            float zPos = player.GetNoteWorldPosition(note, 2).z;
            hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
            hand.CurrentBaseNote = note;

            // Start with thumb or pinky - depending on the upcoming note(s)
            int nextNote = player.GetUpcomingNote();
            int finger = handBase;
            //finger = optimalFingers[player.CurrentNoteIndex];

            if (nextNote < note)
            {
                // Start with pinky
                finger = 4 - handBase;
                //finger = optimalFingers[player.CurrentNoteIndex];
                zPos = player.GetNoteWorldPosition(note, -2).z;
                hand.TargetPositon = new Vector3(hand.transform.position.x, hand.transform.position.y, zPos);
                hand.CurrentBaseNote = note - 7;
            }

            hand.FingerDown(finger);
            hand.LastPlayedNote = note;
            hand.CurrentFingerNotes[finger] = note;

            // Reset hand span
            hand.SetOctaveSpan(0.0f);
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
