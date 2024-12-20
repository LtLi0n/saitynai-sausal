import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  TextField, 
  Button, 
  CircularProgress, 
  Typography, 
  Box, 
  Snackbar, 
  Alert, 
  IconButton, 
  Tooltip 
} from '@mui/material';
import { Add, Delete, ContentCopy } from '@mui/icons-material';
import api from '../services/api';

interface Note {
  id: string;
  content: string;
}

interface Tag {
  id: string;
  tagId: string;
  tagGroupId: string;
  name: string;
}

interface SuggestedTag {
  id: string;
  name: string;
}

const Note: React.FC = () => {
  const { noteId } = useParams<{ noteId: string }>();
  const [content, setContent] = useState<string>('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [tags, setTags] = useState<Tag[]>([]);
  const [newTagGuid, setNewTagGuid] = useState<string>('');
  const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);
  const [snackbarMessage, setSnackbarMessage] = useState<string>('');

  const [suggestedTags, setSuggestedTags] = useState<SuggestedTag[]>([]);
  const [suggestError, setSuggestError] = useState<string | null>(null);

  const navigate = useNavigate();

  useEffect(() => {
    const fetchNote = async () => {
      try {
        if (noteId) {
          const response = await api.get(`/notes/${noteId}`);
          setContent(response.data.content);

          const tagsResponse = await api.get(`/notes/${noteId}/tags`);
          setTags(tagsResponse.data.tags); 
        }
      } catch (error) {
        console.error('Error fetching note or tags:', error);
        setError('Failed to load note. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchNote();
  }, [noteId]);

  const handleSave = async () => {
    if (content.length < 10 || content.length > 100000) {
      setError('Note content must be between 10 and 100,000 characters.');
      return;
    }

    try {
      if (noteId) {
        await api.patch(`/notes/${noteId}`, { newContent: content });
        setSnackbarMessage('Note updated successfully!');
        setSnackbarOpen(true);
      } else {
        await api.post('/notes', { content });
        setSnackbarMessage('Note created successfully!');
        setSnackbarOpen(true);
      }
    } catch (error) {
      console.error('Error saving note:', error);
      setError('Failed to save note. Please try again.');
    }
  };

  const handleDelete = async () => {
    if (!noteId) return;

    if (window.confirm('Are you sure you want to delete this note?')) {
      try {
        await api.delete(`/notes/${noteId}`);
        setSnackbarMessage('Note deleted successfully!');
        setSnackbarOpen(true);
        navigate('/notes');
      } catch (error) {
        console.error('Error deleting note:', error);
        setError('Failed to delete note. Please try again.');
      }
    }
  };

  const handleEmbed = async () => {
    if (!noteId) return;

    try {
      await api.post(`/notes/${noteId}/compute-embedding`);
      setSnackbarMessage('Embedding calculated successfully!');
      setSnackbarOpen(true);
    } catch (error) {
      console.error('Error computing embedding:', error);
      setSnackbarMessage('Failed to compute embedding. Please try again.');
      setSnackbarOpen(true);
    }
  }

  const handleAddTag = async () => {
    if (!newTagGuid || !noteId) {
      setSnackbarMessage('Please enter a valid tag GUID.');
      setSnackbarOpen(true);
      return;
    }

    try {
      await api.post(`/notes/${noteId}/tags/${newTagGuid}`);
      setSnackbarMessage('Tag added successfully!');
      setSnackbarOpen(true);
      const updatedTags = await api.get(`/notes/${noteId}/tags`);
      setTags(updatedTags.data.tags);
      setNewTagGuid('');
    } catch (error) {
      console.error('Error adding tag:', error);
      setSnackbarMessage('Failed to add tag. Please try again.');
      setSnackbarOpen(true);
    }
  };

  const handleRemoveTag = async (tagId: string) => {
    if (!noteId) return;

    try {
      await api.delete(`/notes/${noteId}/tags/${tagId}`);
      setSnackbarMessage('Tag removed successfully!');
      setSnackbarOpen(true);
      const updatedTags = await api.get(`/notes/${noteId}/tags`);
      setTags(updatedTags.data.tags);
    } catch (error) {
      console.error('Error removing tag:', error);
      setSnackbarMessage('Failed to remove tag. Please try again.');
      setSnackbarOpen(true);
    }
  };

  const handleSuggestTags = async () => {
    if (!noteId) return;

    try {
      const response = await api.get(`/notes/${noteId}/suggest-tags`);
      setSuggestedTags(response.data);
      setSuggestError(null);
    } catch (error) {
      console.error('Error suggesting tags:', error);
      setSuggestError('Failed to fetch suggested tags. Please try again.');
    }
  };

  const handleCopyGuid = async (guid: string) => {
    try {
      await navigator.clipboard.writeText(guid);
      setSnackbarMessage(`Copied GUID: ${guid}`);
      setSnackbarOpen(true);
    } catch (e) {
      console.error('Failed to copy guid:', e);
      setSnackbarMessage('Failed to copy GUID. Please try again.');
      setSnackbarOpen(true);
    }
  };

  const handleSnackbarClose = () => {
    setSnackbarOpen(false);
  };

  if (noteId && loading) {
    return (
      <Box display="flex" justifyContent="center" marginTop="2rem">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box padding="2rem" minWidth={600}>
      <Typography variant="h4" component="h1" gutterBottom>
        {noteId ? 'Edit Note' : 'Create New Note'}
      </Typography>
      <TextField
        label="Note Content"
        multiline
        fullWidth
        minRows={10}
        value={content}
        onChange={(e) => {
          setContent(e.target.value);
          setError(null);
        }}
        variant="outlined"
        margin="normal"
        helperText={error}
        error={!!error}
      />
      <Box display="flex" justifyContent="space-between" marginTop="1rem">
        <Button variant="contained" color="secondary" onClick={() => navigate('/notes')}>
          Go back
        </Button>
        {noteId && (
          <Button variant="contained" color="error" onClick={handleDelete}>
            Delete
          </Button>
        )}
        {noteId && (
          <Button variant="contained" color="warning" onClick={handleEmbed}>
            Embed
          </Button>
        )}
        <Button variant="contained" color="primary" onClick={handleSave}>
          {noteId ? 'Save Changes' : 'Create Note'}
        </Button>
      </Box>
      {/* Tags */}
      {noteId && (
        <div>
          <Typography variant="h6" gutterBottom marginTop={5}>
            Tags
          </Typography>
          <Box marginBottom="1rem">
            {tags.map((tag) => (
              <Box
                key={tag.id}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  marginBottom: '0.5rem',
                }}
              >
                <Typography>{tag.name}</Typography>
                <IconButton color="error" onClick={() => handleRemoveTag(tag.tagId)}>
                  <Delete />
                </IconButton>
              </Box>
            ))}
          </Box>
          <Box display="flex" alignItems="center" marginBottom="1rem">
            <TextField
              label="Tag GUID"
              value={newTagGuid}
              onChange={(e) => setNewTagGuid(e.target.value)}
              variant="outlined"
              size="small"
              sx={{ marginRight: '1rem', flex: 1 }}
            />
            <Button
              variant="contained"
              color="primary"
              startIcon={<Add />}
              onClick={handleAddTag}
            >
              Add Tag
            </Button>
          </Box>

          {/* Suggest Tags Section */}
          <Typography variant="h6" gutterBottom marginTop={5}>
            Suggested Tags
          </Typography>
          <Box marginBottom="1rem">
            <Button variant="contained" onClick={handleSuggestTags} sx={{ marginBottom: '1rem' }}>
              Suggest Tags
            </Button>
            {suggestError && <Typography color="error">{suggestError}</Typography>}
            {suggestedTags.map((sTag) => (
              <Box
                key={sTag.id}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  marginBottom: '0.5rem',
                }}
              >
                <Typography>{sTag.name}</Typography>
                <Box>
                  <Tooltip title="Copy GUID">
                    <IconButton color="primary" onClick={() => handleCopyGuid(sTag.id)}>
                      <ContentCopy />
                    </IconButton>
                  </Tooltip>
                </Box>
              </Box>
            ))}
          </Box>
        </div>
      )}

      <Snackbar
        open={snackbarOpen}
        autoHideDuration={3000}
        onClose={handleSnackbarClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleSnackbarClose} severity="success" sx={{ width: '100%' }}>
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Note;
