using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.XR.Haptics
{
    public struct HapticState
    {
        public HapticState(uint samplesQueued, uint samplesAvailable)
        {
            this.samplesQueued = samplesQueued;
            this.samplesAvailable = samplesAvailable;
        }

        public uint samplesQueued { get; private set; }
        public uint samplesAvailable { get; private set; }
    }

    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct GetCurrentHapticStateCommand : IInputDeviceCommandInfo
    {
        static FourCC Type => new FourCC('X', 'H', 'S', '0');

        const int kSize = InputDeviceCommand.kBaseCommandSize + (sizeof(uint) * 2);

        public FourCC typeStatic => Type;

        [FieldOffset(0)]
        InputDeviceCommand baseCommand;

        [FieldOffset(InputDeviceCommand.kBaseCommandSize)]
        public uint samplesQueued;

        [FieldOffset(InputDeviceCommand.kBaseCommandSize + sizeof(int))]
        public uint samplesAvailable;

        public HapticState currentState => new HapticState(samplesQueued, samplesAvailable);

        public static GetCurrentHapticStateCommand Create()
        {
            return new GetCurrentHapticStateCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize),
            };
        }
    }
}
