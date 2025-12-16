using System;
using System.Collections.Generic;
using System.Linq;

namespace ChipEight.Machine.Instructions;

public sealed class InstructionSet
{
    private readonly Dictionary<byte, List<Instruction>> _set = new();

    public InstructionSet Build(Chip chip)
    {
        RegisterPrimary(0x0, new InstructionClear(chip));
        RegisterPrimary(0x0, new InstructionReturn(chip));
        RegisterPrimary(0x1, new InstructionJump(chip));
        RegisterPrimary(0x2, new InstructionCallAddress(chip));
        RegisterPrimary(0x3, new InstructionSkipIfEqual(chip));
        RegisterPrimary(0x4, new InstructionSkipIfNotEqual(chip));
        RegisterPrimary(0x5, new InstructionSkipIfRegistersEqual(chip));
        RegisterPrimary(0x6, new InstructionLoadVxImmediate(chip));
        RegisterPrimary(0x7, new InstructionAddImmediateValueToRegister(chip));
        RegisterPrimary(0x8, new InstructionRegistersMove(chip));
        RegisterPrimary(0x8, new InstructionRegistersOr(chip));
        RegisterPrimary(0x8, new InstructionRegistersAnd(chip));
        RegisterPrimary(0x8, new InstructionRegistersXor(chip));
        RegisterPrimary(0x8, new InstructionRegistersAdd(chip));
        RegisterPrimary(0x8, new InstructionRegistersSubtract(chip));
        RegisterPrimary(0x8, new InstructionRegistersSubtractReverse(chip));
        RegisterPrimary(0x8, new InstructionRegistersShiftRight(chip));
        RegisterPrimary(0x8, new InstructionRegistersShiftLeft(chip));
        RegisterPrimary(0x9, new InstructionSkipIfRegistersNotEqual(chip));
        RegisterPrimary(0xA, new InstructionAddressToI(chip));
        RegisterPrimary(0xB, new InstructionJumpToAddress(chip));
        RegisterPrimary(0xC, new InstructionRandomToRegister(chip));
        RegisterPrimary(0xD, new InstructionDrawSprite(chip));
        RegisterPrimary(0xE, new InstructionSkipIfKey(chip));
        RegisterPrimary(0xE, new InstructionSkipIfNotKey(chip));
        RegisterPrimary(0xF, new InstructionGetDelayTimer(chip));
        RegisterPrimary(0xF, new InstructionSetDelayTimer(chip));
        RegisterPrimary(0xF, new InstructionWaitForKeyPress(chip));
        RegisterPrimary(0xF, new InstructionSetSoundTimer(chip));
        RegisterPrimary(0xF, new InstructionAddRegisterToI(chip));
        RegisterPrimary(0xF, new InstructionSetFontSpriteAddress(chip));
        RegisterPrimary(0xF, new InstructionStoreRegistersToMemory(chip));
        RegisterPrimary(0xF, new InstructionLoadRegistersFromMemory(chip));
        RegisterPrimary(0xF, new InstructionBinaryCodedDecimal(chip));

        return this;
    }

    public void RegisterPrimary(byte index, Instruction instruction)
    {
        if (!_set.ContainsKey(index))
        {
            _set[index] = new List<Instruction>();
        }

        _set[index].Add(instruction);
    }
    
    public void Execute(ushort opcode)
    {
        var high = (byte) (opcode >> 12);
        var instructions = _set[high];
        var instruction = instructions.FirstOrDefault(i => i.CanExecute(opcode));

        if (instruction is null)
        {
            throw new InvalidOperationException($"Unknown opcode: 0x{opcode:X4}");
        }

        instruction.Execute(opcode);
    }
}