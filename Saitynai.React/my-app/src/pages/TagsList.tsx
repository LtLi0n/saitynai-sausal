import React, { useEffect, useState } from 'react';
import { jwtDecode } from 'jwt-decode';
import {
	List,
	ListItemText,
	Collapse,
	Typography,
	ListItemButton,
	Box,
	CircularProgress,
	Button,
	Dialog,
	DialogTitle,
	DialogContent,
	DialogActions,
	TextField,
	IconButton,
	Snackbar,
	Alert,
} from '@mui/material';
import { Add, ArrowLeft, CopyAll, CopyAllRounded, CopyAllSharp, Copyright, CopyrightOutlined, Delete, Edit, ExpandLess, ExpandMore, HorizontalRule, HorizontalSplit, Sync } from '@mui/icons-material';
import api from '../services/api';

interface Tag {
	id: string;
	name: string;
	groupId: string;
	ownerId: string | null;
	isMine: boolean | null;
}

interface TagGroup {
	id: string;
	name: string;
	createdBy: string | null;
	isMine: boolean;
}

const TagsList: React.FC = () => {
	const [tagGroups, setTagGroups] = useState<TagGroup[]>([]);
	const [tags, setTags] = useState<Tag[]>([]);
	const [loading, setLoading] = useState<boolean>(true);
	const [currentParentId, setCurrentParentId] = useState<string | null>(null);
	const [breadcrumbs, setBreadcrumbs] = useState<TagGroup[]>([]);
	const [openDialog, setOpenDialog] = useState<boolean>(false);
	const [alertContent, setAlertContent] = useState<string>('');
	const [dialogMode, setDialogMode] = useState<'createGroup' | 'editGroup' | 'addTag' | 'editTag'>(
		'createGroup'
	);
	const [selectedTag, setSelectedTag] = useState<Tag | null>(null);
	const [selectedGroup, setSelectedGroup] = useState<TagGroup | null>(null);
	const [newName, setNewName] = useState<string>('');
	const [isAdmin, setIsAdmin] = useState<boolean>(false);
	const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);

	useEffect(() => {
		// Fetch current user to determine admin status
		const fetchCurrentUser = async () => {
			try {
				const token = localStorage.getItem('token');
				if (token) {
					const userInfo = jwtDecode(token);
					setIsAdmin(userInfo.is_admin === "true");
				}
			} catch (error) {
				console.error('Error fetching current user:', error);
			}
		};

		fetchCurrentUser();
		fetchTagGroups(currentParentId);
	}, [currentParentId]);

	const syncTags = async () => {
		try{
			const response = await api.post(`/notes/sync-tags`);
			setAlertContent('Tags successfully synced!')
			setSnackbarOpen(true);
		}
		catch(error){
			console.error('Error syncing tags:', error);
		}
	}

	const fetchTagGroups = async (parentId: string | null) => {
		setLoading(true);
		try {
			const response = await api.get(`/tag-groups/list/${parentId || ''}`);
			setTagGroups(response.data);
		} catch (error) {
			console.error('Error fetching tag groups:', error);
		} finally {
			setLoading(false);
		}
	};

	const fetchTags = async (groupId: string) => {
		try {
			const response = await api.get(`/tag-groups/${groupId}/tags`, {
				params: { IncludePublicTags: true },
			});
			setTags(response.data);
		} catch (error) {
			console.error('Error fetching tags:', error);
		}
	};

	const navigateToGroup = (group: TagGroup) => {
		setCurrentParentId(group.id);
		setBreadcrumbs((prev) => [...prev, group]);
		fetchTags(group.id);
	};

	const navigateBack = () => {
		const updatedBreadcrumbs = [...breadcrumbs];
		updatedBreadcrumbs.pop();
		setBreadcrumbs(updatedBreadcrumbs);

		const parentId = updatedBreadcrumbs.length
			? updatedBreadcrumbs[updatedBreadcrumbs.length - 1].id
			: null;
		setCurrentParentId(parentId);

		if (parentId) {
			fetchTags(parentId);
		} else {
			setTags([]);
		}
	};

	const handleOpenDialog = (
		mode: 'createGroup' | 'editGroup' | 'addTag' | 'editTag',
		group: TagGroup | null = null,
		tag: Tag | null = null
	) => {
		setDialogMode(mode);
		setSelectedGroup(group);
		setSelectedTag(tag);
		setNewName(mode === 'editGroup' && group ? group.name : tag ? tag.name : '');
		setOpenDialog(true);
	};

	const handleCreateOrEdit = async () => {
		try {
			if (dialogMode === 'createGroup') {
				await api.post('/tag-groups', {
					name: newName,
					parentGroupId: currentParentId,
				});
				fetchTagGroups(currentParentId);
			} else if (dialogMode === 'editGroup' && selectedGroup) {
				await api.patch(`/tag-groups/${selectedGroup.id}`, {
					name: newName,
				});
				fetchTagGroups(currentParentId);
			} else if (dialogMode === 'addTag' && currentParentId) {
				await api.post(`/tag-groups/${currentParentId}/tags`, {
					name: newName,
				});
				fetchTags(currentParentId);
			} else if (dialogMode === 'editTag' && selectedTag) {
				await api.patch(`/tag-groups/tags/${selectedTag.id}`, {
					name: newName,
				});
				fetchTags(selectedTag.groupId);
			}
			setOpenDialog(false);
		} catch (error) {
			console.error('Error creating or editing:', error);
		}
	};

	const handleDelete = async (type: 'group' | 'tag', id: string) => {
		if (window.confirm('Are you sure you want to delete this?')) {
			try {
				if (type === 'group') {
					await api.delete(`/tag-groups/${id}`);
					fetchTagGroups(currentParentId);
				} else {
					await api.delete(`/tag-groups/tags/${id}`);
					fetchTags(currentParentId!);
				}
			} catch (error) {
				console.error('Error deleting:', error);
			}
		}
	};

	if (loading) {
		return (
			<Box display="flex" justifyContent="center" marginTop="2rem">
				<CircularProgress />
			</Box>
		);
	}

	const canEditOrDelete = (tag: TagGroup | Tag) => {
		return isAdmin || tag.isMine;
	};

	if (loading) {
		return (
			<Box display="flex" justifyContent="center" marginTop="2rem">
				<CircularProgress />
			</Box>
		);
	}

	return (
		<Box padding="1rem">

			{isAdmin && (<>
				<Button
					variant='contained'
					startIcon={<Sync />}
					color='secondary'
					onClick={syncTags}>Sync tags</Button>
				</>)}

			<Typography variant="h4" gutterBottom marginTop={5}>
				Tags and Groups
			</Typography>

			{/* Breadcrumb Navigation */}
			<Box display="flex" alignItems="center" marginBottom="1rem">
				{breadcrumbs.length > 0 && (
					<Button
						variant="contained"
						onClick={navigateBack}
						startIcon={<ArrowLeft />}
						sx={{ marginRight: '1rem' }}>
						Back
					</Button>
				)}
				<Typography variant="h6">
					{breadcrumbs.length > 0
						? breadcrumbs[breadcrumbs.length - 1].name
						: 'Root Groups'}
				</Typography>
			</Box>

			{/* List of Tag Groups */}
			<List>
				{tagGroups.map((group) => (
					<Box key={group.id} sx={{ marginBottom: '1rem', display: "flex" }}>
						<ListItemButton onClick={() => navigateToGroup(group)} style={{backgroundColor: 'black', color: 'white'}}>
							<ListItemText
								primary={group.name}
								secondary={group.createdBy ? `Created by: ${group.createdBy}` : 'Public'}
							/>
							<ExpandMore />
						</ListItemButton>
						{canEditOrDelete(group) && (
							<Box sx={{ display: 'flex', gap: '0.5rem', paddingLeft: '1rem', border: "black 1px solid" }}>
								<Button
									variant="text"
									color="secondary"
									startIcon={<Edit />}
									onClick={() => handleOpenDialog('editGroup', group)}
								></Button>
								<Button
									variant="text"
									color="error"
									startIcon={<Delete />}
									onClick={() => handleDelete('group', group.id)}
								></Button>

							</Box>
						)}
					</Box>
				))}
			</List>

			{/* Create Group Button */}
			<Button
				variant="contained"
				color="primary"
				sx={{ marginBottom: '1rem' }}
				onClick={() => handleOpenDialog('createGroup')}
			>
				Create Group
			</Button>

			{/* List of Tags */}
			{currentParentId && (
				<div>
					<Box sx={{ borderBottom: '1px solid #ccc', width: '100%' }} />
					<Box marginTop={2}>
						<Typography variant="h5" gutterBottom>
							Tags
						</Typography>
						{tags.length > 0 ? (
							tags.map((tag) => (
								<Box
									key={tag.id}
									sx={{
										display: 'flex',
										alignItems: 'center',
										justifyContent: 'space-between',
										marginBottom: '0.5rem',
									}}
								>
									<Typography variant="body1">{tag.name}</Typography>
									<Box>
										<IconButton
											color="success"
											onClick={() => {
												navigator.clipboard.writeText(tag.id);
												setAlertContent('Tag GUID copied to clipboard!')
												setSnackbarOpen(true);
											}}
										>
											<CopyAllRounded />
										</IconButton>
										{canEditOrDelete(tag) && (
											<>

												<IconButton
													color="primary"
													onClick={() => handleOpenDialog('editTag', null, tag)}
												>
													<Edit />
												</IconButton>
												<IconButton
													color="error"
													onClick={() => handleDelete('tag', tag.id)}
												>
													<Delete />
												</IconButton>
											</>
										)}
									</Box>
								</Box>
							))
						) : (
							<Typography variant="body2" color="textSecondary">
								No tags available.
							</Typography>
						)}
						<Button
							variant="contained"
							color="primary"
							startIcon={<Add />}
							onClick={() => handleOpenDialog('addTag')}
							sx={{ marginTop: '1rem' }}
						>
							Add Tag
						</Button>
					</Box></div>

			)}

			{/* Dialog for Create/Edit */}
			<Dialog open={openDialog} onClose={() => setOpenDialog(false)}>
				<DialogTitle>
					{dialogMode === 'createGroup'
						? 'Create Group'
						: dialogMode === 'editGroup'
							? 'Edit Group'
							: dialogMode === 'addTag'
								? 'Add Tag'
								: 'Edit Tag'}
				</DialogTitle>
				<DialogContent>
					<TextField
						fullWidth
						label={dialogMode.includes('Tag') ? 'Tag Name' : 'Group Name'}
						value={newName}
						onChange={(e) => setNewName(e.target.value)} />
				</DialogContent>
				<DialogActions>
					<Button onClick={() => setOpenDialog(false)}>Cancel</Button>
					<Button variant="contained" onClick={handleCreateOrEdit}>
						Save
					</Button>
				</DialogActions>
			</Dialog>

			{/* Snackbar for Copy Feedback */}
			<Snackbar
				open={snackbarOpen}
				autoHideDuration={3000}
				onClose={() => setSnackbarOpen(false)}
				anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
			>
				<Alert onClose={() => setSnackbarOpen(false)} severity="success" sx={{ width: '100%' }}>
					{alertContent}
				</Alert>
			</Snackbar>
		</Box>
	);
};

export default TagsList;
