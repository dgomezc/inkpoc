﻿using InkPoc.Services.Ink;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;

namespace InkPoc.Helpers.Ink.UndoRedo
{
    public class RemoveStrokeUndoRedoOperation : IUndoRedoOperation
    {
        private List<InkStroke> strokes;
        private readonly InkStrokesService strokeService;

        public RemoveStrokeUndoRedoOperation(IEnumerable<InkStroke> _strokes, InkStrokesService _strokeService)
        {
            strokes = new List<InkStroke>(_strokes);
            strokeService = _strokeService;

            strokeService.AddStrokeEvent += StrokeService_AddStrokeEvent;
        }

        public void ExecuteRedo() => strokes.ForEach(s => strokeService.RemoveStrokeToContainer(s));

        public void ExecuteUndo() => strokes.ToList().ForEach(s => strokeService.AddStrokeToContainer(s));

        private void StrokeService_AddStrokeEvent(object sender, AddStrokeToContainerEventArgs e)
        {
            if (e.NewStroke == null)
            {
                return;
            }

            var changes = strokes.RemoveAll(s => s.Id == e.OldStroke?.Id);
            if (changes > 0)
            {
                strokes.Add(e.NewStroke);
            }
        }
    }
}