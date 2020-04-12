using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;


#pragma warning disable CS0649, CA1812, CA2211,CA1707
namespace AK.Wwise.Waapi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
    public class WwiseObjectsReference
    {
        // TOBEDELETED:
        /*
        public WwiseObjectsReference() { }
        public static string[] GetPropertyNames()
        {
            return typeof(WwiseObjectsReference).GetFields(System.Reflection.BindingFlags.Public).Select(n => n.Name).ToArray();
        }
        */
        private const string DefaultStr = "";
        private const string DefaultRefID = "{ 00000000 - 0000 - 0000 - 0000 - 000000000000 }";

        // @ or none = そのプロパティに設定してある値を返す(Override関係なし)。
        // @@ = そのプロパティの実際の値を返す。Overrideしていなかった場合はParentの値になる。
        // General Settingss - built-in accessors
        public string id { get; set; }
        public string name { get; set; }
        [DefaultValue(DefaultStr)]
        public string notes { get; set; }
        public string type { get; set; }
        [DefaultValue(DefaultRefID)]
        public string parent { get; set; }
        public string childrenCount { get; set; }

        // General Settings - object reference
        [DefaultValue(true)]
        public bool _Inclusion { get; set; }
        public short _Color { get; set; }

        public double _Volume { get; set; }
        public short _Lowpass { get; set; }
        public short _Highpass { get; set; }
        public int _Pitch { get; set; }
        public double _InitialDelay { get; set; }
        public bool _OverrideOutput { get; set; }

        [DefaultValue(DefaultRefID)]
        public string _OutputBus { get; set; }
        public double _OutputBusVolume { get; set; }
        public short _OutputBusLowpass { get; set; }
        public short _OutputBusHighpass { get; set; }

        public bool _UseGameAuxSends { get; set; }
        public bool _OverrideGameAuxSends { get; set; }
        public double _GameAuxSendVolume { get; set; }
        public short _GameAuxSendLPF { get; set; }
        public short _GameAuxSendHPF { get; set; }

        public bool _OverrideUserAuxSends { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _UserAuxSend0 { get; set; }
        public double _UserAuxSendVolume0 { get; set; }
        public short _UserAuxSendLPF0 { get; set; }
        public short _UserAuxSendHPF0 { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _UserAuxSend1 { get; set; }
        public double _UserAuxSendVolume1 { get; set; }
        public short _UserAuxSendLPF1 { get; set; }
        public short _UserAuxSendHPF1 { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _UserAuxSend2 { get; set; }
        public double _UserAuxSendVolume2 { get; set; }
        public short _UserAuxSendLPF2 { get; set; }
        public short _UserAuxSendHPF2 { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _UserAuxSend3 { get; set; }
        public double _UserAuxSendVolume3 { get; set; }
        public short _UserAuxSendLPF3 { get; set; }
        public short _UserAuxSendHPF3 { get; set; }

        public bool _OverrideEarlyReflections { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _ReflectionsAuxSend { get; set; }
        public double _ReflectionsVolume { get; set; }

        // Conversion
        public bool _OverrideConversion { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _Conversion { get; set; }
        public bool _OverrideAnalysis { get; set; }
        public bool _EnableLoudnessNormalization { get; set; }
        public double _MakeUpGain { get; set; }
        // Effects
        public bool _OverrideEffect { get; set; }
        public bool _BypassEffect { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _Effect0 { get; set; }
        public bool _RenderEffect0 { get; set; }
        public bool _BypassEffect0 { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _Effect1 { get; set; }
        public bool _RenderEffect1 { get; set; }
        public bool _BypassEffect1 { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _Effect2 { get; set; }
        public bool _RenderEffect2 { get; set; }
        public bool _BypassEffect2 { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _Effect3 { get; set; }
        public bool _RenderEffect3 { get; set; }
        public bool _BypassEffect3 { get; set; }
        // Positioning
        public bool _OverridePositioning { get; set; }
        public int _CenterPercentage { get; set; }
        public short _SpeakerPanning { get; set; }
        [DefaultValue(true)]
        public bool _ListenerRelativeRouting { get; set; }
        public short _3DSpatialization { get; set; }
        [DefaultValue(100)]
        public int _SpeakerPanning3DSpatializationMix { get; set; }
        [DefaultValue(true)]
        public bool _EnableAttenuation { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _Attenuation { get; set; }
        public short _3DPosition { get; set; }
        public bool _HoldListenerOrientation { get; set; }
        public bool _HoldEmitterPositionOrientation { get; set; }
        public bool _EnableDiffraction { get; set; }
        // HDR
        public bool _OverrideHdrEnvelope { get; set; }
        public bool _HdrEnableEnvelope { get; set; }
        [DefaultValue(20)]
        public double _HdrEnvelopeSensitivity { get; set; }
        [DefaultValue(12)]
        public double _HdrActiveRange { get; set; }
        // Mixer Plug-in
        public bool _OverrideAttachableMixerInput { get; set; }
        [DefaultValue(DefaultRefID)]
        public string _AttachableMixerInput { get; set; }
        // MIDI
        public bool _OverrideMidiEventsBehavior { get; set; }
        [DefaultValue(1)]
        public short _MidiPlayOnNoteType { get; set; }
        public bool _MidiBreakOnNoteOff { get; set; }
        public bool _OverrideMidiNoteTracking { get; set; }
        public bool _EnableMidiNoteTracking { get; set; }
        [DefaultValue(60)]
        public int _MidiTrackingRootNote { get; set; }
        public int _MidiTransposition { get; set; }
        public int _MidiVelocityOffset { get; set; }
        [DefaultValue(127)]
        public int _MidiKeyFilterMax { get; set; }
        public int _MidiKeyFilterMin { get; set; }
        [DefaultValue(127)]
        public int _MidiVelocityFilterMax { get; set; }
        public int _MidiVelocityFilterMin { get; set; }
        [DefaultValue(65535)]
        public int _MidiChannelFilter { get; set; }
        // Advansed Settings - Playback Limit
        public bool _IgnoreParentMaxSoundInstance { get; set; }
        public bool _UseMaxSoundPerInstance { get; set; }
        [DefaultValue(50)]
        public short _MaxSoundPerInstance { get; set; }
        public short _IsGlobalLimit { get; set; }
        public bool _OverLimitBehavior { get; set; }
        public short _MaxReachedBehavior { get; set; }
        // Advansed Settings - Virtual Voice
        public bool _OverrideVirtualVoice { get; set; }
        public short _BelowThresholdBehavior { get; set; }
        [DefaultValue(1)]
        public short _VirtualVoiceQueueBehavior { get; set; }
        // Advansed Settings - Playback Priority
        public bool _OverridePriority { get; set; }
        [DefaultValue(50)]
        public short _Priority { get; set; }
        public bool _PriorityDistanceFactor { get; set; }
        [DefaultValue(-10)]
        public short _PriorityDistanceOffset { get; set; }

        /*
         * TODO: how to handle Container Properies...
        // Common Property for Container
        [DefaultValue(50)]
        public double _Weight { get; set; }
        // BlendContainer
        public short _BlendBehavior { get; set; 
        // RandomSequenceContainer
        public short _GlobalOrPerObject { get; set; }   */
        public short _RandomOrSequence { get; set; }
        /*        public short _NormalOrShuffle { get; set; }
                [DefaultValue(true)]
                public bool _RandomAvoidRepeating { get; set; }
                public int _RandomAvoidRepeatingCount { get; set; }
                public short _RestartBeginningOrBackward { get; set; }
                public short _PlayMechanismStepOrContinuous { get; set; }
                [DefaultValue(true)]
                public bool _PlayMechanismResetPlaylistEachPlay { get; set; }
                public bool _PlayMechanismLoop { get; set; }
                public short _PlayMechanismInfiniteOrNumberOfLoops { get; set; }
                public int _PlayMechanismLoopCount { get; set; }
                public bool _PlayMechanismSpecialTransitions { get; set; }
                public short _PlayMechanismSpecialTransitionsType { get; set; }
                public double _PlayMechanismSpecialTransitionsValue { get; set; }
                // SwitchContainer
                public short _SwitchBehavior { get; set; }
                [DefaultValue(DefaultRefID)]
                public string _SwitchGroupOrStateGroup { get; set; }
                [DefaultValue(DefaultRefID)]
                public string _DefaultSwitchOrState { get; set; }
                */
    }
}
#pragma warning restore CS0649, CA1812, CA2211, CA1707