using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;


#pragma warning disable CS0649, CA1812, CA2211
namespace AK.Wwise.Waapi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
    public class WwiseObjectsReference
    {
        public static string[] GetPropertyNames()
        {
            return typeof(WwiseObjectsReference).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Select(n => n.Name).ToArray();
        }

        private const string DefaultStr = "";
        private const string DefaultRefID = "{ 00000000 - 0000 - 0000 - 0000 - 000000000000 }";


        // General Settings - built-in accessors
        public static string name;
        [DefaultValue(DefaultStr)]
        public static string notes;
        public static string type;
        [DefaultValue(DefaultRefID)]
        public static string parent;

        // General Settings - object reference
        [DefaultValue(true)]
        public static bool _Inclusion;
        public static short _Color;

        public static double _Volume;
        public static short _Lowpass;
        public static short _Highpass;
        public static int _Pitch;
        public static double _InitialDelay;
        public static bool _OverrideOutput;

        [DefaultValue(DefaultRefID)]
        public static string _OutputBus;
        public static double _OutputBusVolume;
        public static short _OutputBusLowpass;
        public static short _OutputBusHighpass;

        public static bool _UseGameAuxSends;
        public static bool _OverrideGameAuxSends;
        public static double _GameAuxSendVolume;
        public static short _GameAuxSendLPF;
        public static short _GameAuxSendHPF;

        public static bool _OverrideUserAuxSends;
        [DefaultValue(DefaultRefID)]
        public static string _UserAuxSend0;
        public static double _UserAuxSendVolume0;
        public static short _UserAuxSendLPF0;
        public static short _UserAuxSendHPF0;
        [DefaultValue(DefaultRefID)]
        public static string _UserAuxSend1;
        public static double _UserAuxSendVolume1;
        public static short _UserAuxSendLPF1;
        public static short _UserAuxSendHPF1;
        [DefaultValue(DefaultRefID)]
        public static string _UserAuxSend2;
        public static double _UserAuxSendVolume2;
        public static short _UserAuxSendLPF2;
        public static short _UserAuxSendHPF2;
        [DefaultValue(DefaultRefID)]
        public static string _UserAuxSend3;
        public static double _UserAuxSendVolume3;
        public static short _UserAuxSendLPF3;
        public static short _UserAuxSendHPF3;

        public static bool _OverrideEarlyReflections;
        [DefaultValue(DefaultRefID)]
        public static string _ReflectionsAuxSend;
        public static double _ReflectionsVolume;

        // Conversion
        public static bool _OverrideConversion;
        [DefaultValue(DefaultRefID)]
        public static string _Conversion;
        public static bool _OverrideAnalysis;
        public static bool _EnableLoudnessNormalization;
        public static double _MakeUpGain;
        // Effects
        public static bool _OverrideEffect;
        public static bool _BypassEffect;
        [DefaultValue(DefaultRefID)]
        public static string _Effect0;
        public static bool _RenderEffect0;
        public static bool _BypassEffect0;
        [DefaultValue(DefaultRefID)]
        public static string _Effect1;
        public static bool _RenderEffect1;
        public static bool _BypassEffect1;
        [DefaultValue(DefaultRefID)]
        public static string _Effect2;
        public static bool _RenderEffect2;
        public static bool _BypassEffect2;
        [DefaultValue(DefaultRefID)]
        public static string _Effect3;
        public static bool _RenderEffect3;
        public static bool _BypassEffect3;
        // Positioning
        public static bool _OverridePositioning;
        public static int _CenterPercentage;
        public static short _SpeakerPanning;
        [DefaultValue(true)]
        public static bool _ListenerRelativeRouting;
        public static short _3DSpatialization;
        [DefaultValue(100)]
        public static int _SpeakerPanning3DSpatializationMix;
        [DefaultValue(true)]
        public static bool _EnableAttenuation;
        [DefaultValue(DefaultRefID)]
        public static string _Attenuation;
        public static short _3DPosition;
        public static bool _HoldListenerOrientation;
        public static bool _HoldEmitterPositionOrientation;
        public static bool _EnableDiffraction;
        // HDR
        public static bool _OverrideHdrEnvelope;
        public static bool _HdrEnableEnvelope;
        [DefaultValue(20)]
        public static double _HdrEnvelopeSensitivity;
        [DefaultValue(12)]
        public static double _HdrActiveRange;
        // Mixer Plug-in
        public static bool _OverrideAttachableMixerInput;
        [DefaultValue(DefaultRefID)]
        public static string _AttachableMixerInput;
        // MIDI
        public static bool _OverrideMidiEventsBehavior;
        [DefaultValue(1)]
        public static short _MidiPlayOnNoteType;
        public static bool _MidiBreakOnNoteOff;
        public static bool _OverrideMidiNoteTracking;
        public static bool _EnableMidiNoteTracking;
        [DefaultValue(60)]
        public static int _MidiTrackingRootNote;
        public static int _MidiTransposition;
        public static int _MidiVelocityOffset;
        [DefaultValue(127)]
        public static int _MidiKeyFilterMax;
        public static int _MidiKeyFilterMin;
        [DefaultValue(127)]
        public static int _MidiVelocityFilterMax;
        public static int _MidiVelocityFilterMin;
        [DefaultValue(65535)]
        public static int _MidiChannelFilter;
        // Advansed Settings - Playback Limit
        public static bool _IgnoreParentMaxSoundInstance;
        public static bool _UseMaxSoundPerInstance;
        [DefaultValue(50)]
        public static short _MaxSoundPerInstance;
        public static short _IsGlobalLimit;
        public static bool _OverLimitBehavior;
        public static short _MaxReachedBehavior;
        // Advansed Settings - Virtual Voice
        public static bool _OverrideVirtualVoice;
        public static short _BelowThresholdBehavior;
        [DefaultValue(1)]
        public static short _VirtualVoiceQueueBehavior;
        // Advansed Settings - Playback Priority
        public static bool _OverridePriority;
        [DefaultValue(50)]
        public static short _Priority;
        public static bool _PriorityDistanceFactor;
        [DefaultValue(-10)]
        public static short _PriorityDistanceOffset;
        // Common Property for Container
        [DefaultValue(50)]
        public static double _Weight;
        // BlendContainer
        public static short _BlendBehavior;
        // RandomSequenceContainer
        public static short _GlobalOrPerObject;
        public static short _RandomOrSequence;
        public static short _NormalOrShuffle;
        [DefaultValue(true)]
        public static bool _RandomAvoidRepeating;
        public static int _RandomAvoidRepeatingCount;
        public static short _RestartBeginningOrBackward;
        public static short _PlayMechanismStepOrContinuous;
        [DefaultValue(true)]
        public static bool _PlayMechanismResetPlaylistEachPlay;
        public static bool _PlayMechanismLoop;
        public static short _PlayMechanismInfiniteOrNumberOfLoops;
        public static int _PlayMechanismLoopCount;
        public static bool _PlayMechanismSpecialTransitions;
        public static short _PlayMechanismSpecialTransitionsType;
        public static double _PlayMechanismSpecialTransitionsValue;
        // SwitchContainer
        public static short _SwitchBehavior;
        [DefaultValue(DefaultRefID)]
        public static string _SwitchGroupOrStateGroup;
        [DefaultValue(DefaultRefID)]
        public static string _DefaultSwitchOrState;
    }
}
#pragma warning restore CS0649, CA1812, CA2211