﻿using CSharpSynth.Sequencer;
using CSharpSynth.Synthesis;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayPianoLegacy : MonoBehaviour {

    public float keyPressOffset = 0.02f;

    // Midi
    public string midiFilePath = "Midis/fur_elise.mid";
    public string bankFilePath = "GM Bank/gm";
    public int bufferSize = 1024;

    private float[] sampleBuffer;
    private float gain = 1f;
    private MidiSequencer midiSequencer;
    private StreamSynthesizer midiStreamSynthesizer;

    public GameObject keyboardObject;

    private GameObject[] noteObjects;
    private bool[] notePressed;
    private bool[] previousNotePressed;
    private float defaultKeyWhiteY;
    private float defaultKeyBlackY;

    public event Action<int> OnNotePress;
    public event Action<int> OnNoteRelease;

    // Awake is called when the script instance
    // is being loaded.
    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		midiStreamSynthesizer = new StreamSynthesizer(44100, 1, bufferSize, 40);
#else
        midiStreamSynthesizer = new StreamSynthesizer(44100, 2, bufferSize, 40);
#endif
        sampleBuffer = new float[midiStreamSynthesizer.BufferSize];
        midiStreamSynthesizer.LoadBank(bankFilePath);
        midiSequencer = new MidiSequencer(midiStreamSynthesizer);
        midiSequencer.TempoScale = 1.0f;

        //These will be fired by the midiSequencer when a song plays
        midiSequencer.NoteOnEvent += new MidiSequencer.NoteOnEventHandler(MidiNoteOnHandler);
        midiSequencer.NoteOffEvent += new MidiSequencer.NoteOffEventHandler(MidiNoteOffHandler);

        // Set Default tuning adjustment to about -1 halftone
        //midiStreamSynthesizer.TuningAdjustment = -1.0f + AppProperties.TuningAdjustment;

    }

    // Start is called just before any of the
    // Update methods is called the first time.
    void Start()
    {   
        noteObjects = new GameObject[CreatePiano.KEY_COUNT];
        notePressed = new bool[CreatePiano.KEY_COUNT];
        previousNotePressed = new bool[CreatePiano.KEY_COUNT];
        for (int i = 0; i < CreatePiano.KEY_COUNT; i++)
        {
            noteObjects[i] = keyboardObject.gameObject.transform.GetChild(i).gameObject;
            notePressed[i] = false;
            previousNotePressed[i] = false;
        }

        defaultKeyWhiteY = noteObjects[0].transform.position.y;
        defaultKeyBlackY = noteObjects[1].transform.position.y;

        StartPlayback();

    }

    public Vector3 GetNoteWorldPosition(int note)
    {
        return noteObjects[note].transform.position;
    }

    // Returns the note position + an offset of n white keys
    public Vector3 GetNoteWorldPosition(int note, int whiteKeyOffset)
    {
        int newNote = note;
        int i = 0;
        while (i < whiteKeyOffset)
        {
            newNote++;
            if (!IsBlackKey(newNote))
                i++;
        }

        return GetNoteWorldPosition(newNote);
    }

    public bool IsBlackKey(int key)
    {
        int octaveKey = key % 12;
        return (octaveKey == 1 || octaveKey == 3 || octaveKey == 6 || octaveKey == 8 || octaveKey == 10);
    }

    public int GetKeyPosByNoteIndex(int note)
    {
        int keyPos = 0;
        for (int i = 0; i < note; i++)
        {
            if (!IsBlackKey(i))
            {
                keyPos++;
            }
        }
        return keyPos;
    }

    private void StartPlayback()
    {
        midiSequencer.LoadMidi(midiFilePath, false);

        midiSequencer.Play();
    }
    
    // Update is called every frame, if the
    // MonoBehaviour is enabled.
    void Update()
    {
        // Check note pressed
        for (int i = 0; i < CreatePiano.KEY_COUNT; i++)
        {
            GameObject currentNote = noteObjects[i];
            if (notePressed[i])
            {
                if (IsBlackKey(i))
                    currentNote.transform.position = new Vector3(currentNote.transform.position.x, defaultKeyBlackY - keyPressOffset, currentNote.transform.position.z);
                else
                    currentNote.transform.position = new Vector3(currentNote.transform.position.x, defaultKeyWhiteY - keyPressOffset, currentNote.transform.position.z);

                if (!previousNotePressed[i])
                {
                    // New key press
                    if (OnNotePress != null)
                        OnNotePress(i);
                }
            }
            else
            {
                if (IsBlackKey(i))
                    currentNote.transform.position = new Vector3(currentNote.transform.position.x, defaultKeyBlackY, currentNote.transform.position.z);
                else
                    currentNote.transform.position = new Vector3(currentNote.transform.position.x, defaultKeyWhiteY, currentNote.transform.position.z);

                if (previousNotePressed[i])
                {
                    // New key release
                    if (OnNoteRelease != null)
                        OnNoteRelease(i);
                }
            }

            previousNotePressed[i] = notePressed[i];
        }
    }

    // This function is called when the object
    // becomes enabled and active.
    void OnEnable()
    {

    }

    // This function is called when the behaviour
    // becomes disabled () or inactive.
    void OnDisable()
    {

    }

    // Reset to default values.
    void Reset()
    {

    }

    // See http://unity3d.com/support/documentation/ScriptReference/MonoBehaviour.OnAudioFilterRead.html for reference code
    //	If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain.
    //
    //	The filter is inserted in the same order as the MonoBehaviour script is shown in the inspector. 	
    //	OnAudioFilterRead is called everytime a chunk of audio is routed thru the filter (this happens frequently, every ~20ms depending on the samplerate and platform). 
    //	The audio data is an array of floats ranging from [-1.0f;1.0f] and contains audio from the previous filter in the chain or the AudioClip on the AudioSource. 
    //	If this is the first filter in the chain and a clip isn't attached to the audio source this filter will be 'played'. 
    //	That way you can use the filter as the audio clip, procedurally generating audio.
    //
    //	If OnAudioFilterRead is implemented a VU meter will show up in the inspector showing the outgoing samples level. 
    //	The process time of the filter is also measured and the spent milliseconds will show up next to the VU Meter 
    //	(it turns red if the filter is taking up too much time, so the mixer will starv audio data). 
    //	Also note, that OnAudioFilterRead is called on a different thread from the main thread (namely the audio thread) 
    //	so calling into many Unity functions from this function is not allowed ( a warning will show up ). 	
    private void OnAudioFilterRead(float[] data, int channels)
    {
        //This uses the Unity specific float method we added to get the buffer
        midiStreamSynthesizer.GetNext(sampleBuffer);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = sampleBuffer[i] * gain;
        }
    }

    public void MidiNoteOnHandler(int channel, int note, int velocity)
    {
        int idx = note - 24;
        if (idx > CreatePiano.KEY_COUNT)
            idx = CreatePiano.KEY_COUNT;

        notePressed[idx] = true;
        
    }

    public void MidiNoteOffHandler(int channel, int note)
    {
        int idx = note - 24;
        if (idx > CreatePiano.KEY_COUNT)
            idx = CreatePiano.KEY_COUNT;

        notePressed[idx] = false;

    }

}
