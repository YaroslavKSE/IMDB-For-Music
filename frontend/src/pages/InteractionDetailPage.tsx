import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Heart, MessageSquare, SlidersHorizontal, Trash2, Flag, Send, ThumbsUp, Calendar } from 'lucide-react';
import InteractionService, { InteractionDetailDTO, ReviewComment } from '../api/interaction';
import CatalogService from '../api/catalog';
import useAuthStore from '../store/authStore';
import UsersService, { UserSummary } from '../api/users';
import { formatDate } from '../utils/formatters';
import NormalizedStarDisplay from '../components/CreateInteraction/NormalizedStarDisplay';
import ComplexRatingModal from '../components/Diary/ComplexRatingModal';

const InteractionDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuthStore();

    const [interaction, setInteraction] = useState<InteractionDetailDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [catalogItem, setCatalogItem] = useState<any>(null);
    const [creatorProfile, setCreatorProfile] = useState<any>(null);
    const [comments, setComments] = useState<ReviewComment[]>([]);
    const [commentUsers, setCommentUsers] = useState<Map<string, UserSummary>>(new Map());
    const [commentsLoading, setCommentsLoading] = useState(false);
    const [commentsError, setCommentsError] = useState<string | null>(null);
    const [hasMoreComments, setHasMoreComments] = useState(false);
    const [commentsOffset, setCommentsOffset] = useState(0);
    const [totalComments, setTotalComments] = useState(0);
    const [newComment, setNewComment] = useState('');
    const [submittingComment, setSubmittingComment] = useState(false);
    const [hasLikedReview, setHasLikedReview] = useState(false);
    const [likeCount, setLikeCount] = useState(0);
    const [isRatingModalOpen, setIsRatingModalOpen] = useState(false);
    const [processingLike, setProcessingLike] = useState(false);

    // Fetch interaction data
    useEffect(() => {
        const fetchInteractionData = async () => {
            if (!id) return;

            setLoading(true);
            setError(null);

            try {
                // Get interaction details
                const interactionData = await InteractionService.getInteractionById(id);
                setInteraction(interactionData);

                // Fetch catalog item (album or track)
                let itemData;
                if (interactionData.itemType === 'Album') {
                    itemData = await CatalogService.getAlbum(interactionData.itemId);
                } else if (interactionData.itemType === 'Track') {
                    itemData = await CatalogService.getTrack(interactionData.itemId);
                }
                setCatalogItem(itemData);

                // Fetch creator profile
                const creatorData = await UsersService.getUserProfileById(interactionData.userId);
                setCreatorProfile(creatorData);

                // If there's a review, check if current user has liked it
                if (interactionData.review && isAuthenticated && user) {
                    const likeStatus = await InteractionService.checkReviewLike(
                        interactionData.review.reviewId,
                        user.id
                    );
                    setHasLikedReview(likeStatus);

                    setLikeCount(interactionData.review.likes); // This would be replaced with actual like count from backend
                }
            } catch (err) {
                console.error('Error fetching interaction data:', err);
                setError('Failed to load interaction data. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchInteractionData();
    }, [id, isAuthenticated, user]);

    // Fetch initial comments
    useEffect(() => {
        const fetchComments = async () => {
            if (!interaction?.review?.reviewId) return;

            setCommentsLoading(true);
            setCommentsError(null);

            try {
                const result = await InteractionService.getReviewComments(
                    interaction.review.reviewId,
                    10,
                    0
                );

                setComments(result.comments || []);
                setTotalComments(result.totalCount);
                setHasMoreComments(result.totalCount > 10);
                setCommentsOffset(result.comments?.length || 0);

                // Fetch user data for each comment
                if (result.comments && result.comments.length > 0) {
                    const userIds = [...new Set(result.comments.map(comment => comment.userId))];
                    await fetchUsersForComments(userIds);
                }
            } catch (err) {
                console.error('Error fetching comments:', err);
                setCommentsError('Failed to load comments');
            } finally {
                setCommentsLoading(false);
            }
        };

        if (interaction?.review) {
            fetchComments();
        }
    }, [interaction]);

    // Function to fetch user data for comments
    const fetchUsersForComments = async (userIds: string[]) => {
        try {
            const usersMap = new Map<string, UserSummary>();

            // Fetch each user individually
            // In a production app, you'd want a batch fetch API for better performance
            for (const userId of userIds) {
                try {
                    const userData = await UsersService.getUserProfileById(userId);
                    usersMap.set(userId, {
                        id: userData.id,
                        username: userData.username,
                        name: userData.name,
                        surname: userData.surname,
                        avatarUrl: userData.avatarUrl
                    });
                } catch (error) {
                    console.error(`Error fetching user ${userId}:`, error);
                }
            }

            setCommentUsers(usersMap);
        } catch (error) {
            console.error('Error fetching users for comments:', error);
        }
    };

    const handleLoadMoreComments = async () => {
        if (!interaction?.review?.reviewId || commentsLoading || !hasMoreComments) return;

        setCommentsLoading(true);

        try {
            const result = await InteractionService.getReviewComments(
                interaction.review.reviewId,
                10,
                commentsOffset
            );

            const newComments = result.comments || [];
            setComments((prev) => [...prev, ...newComments]);
            setCommentsOffset((prev) => prev + newComments.length);
            setHasMoreComments((commentsOffset + newComments.length) < result.totalCount);

            // Fetch user data for new comments
            if (newComments.length > 0) {
                const newUserIds = [...new Set(newComments.map(comment => comment.userId))];
                // Filter out users we already have
                const userIdsToFetch = newUserIds.filter(id => !commentUsers.has(id));
                if (userIdsToFetch.length > 0) {
                    await fetchUsersForComments(userIdsToFetch);
                }
            }
        } catch (err) {
            console.error('Error loading more comments:', err);
            setCommentsError('Failed to load more comments');
        } finally {
            setCommentsLoading(false);
        }
    };

    const handleSubmitComment = async () => {
        if (!interaction?.review?.reviewId || !user || !newComment.trim() || submittingComment) return;

        setSubmittingComment(true);

        try {
            const success = await InteractionService.postReviewComment({
                reviewId: interaction.review.reviewId,
                userId: user.id,
                commentText: newComment
            });

            if (success) {
                // Add the new comment to the list (optimistic update)
                const newCommentObj: ReviewComment = {
                    commentId: Date.now().toString(), // Temporary ID
                    reviewId: interaction.review.reviewId,
                    userId: user.id,
                    commentedAt: new Date().toISOString(),
                    commentText: newComment
                };

                // Make sure we have the current user in our users map
                const updatedUsers = new Map(commentUsers);
                if (!updatedUsers.has(user.id)) {
                    updatedUsers.set(user.id, {
                        id: user.id,
                        username: user.username || '',
                        name: user.name,
                        surname: user.surname,
                        avatarUrl: user.avatarUrl
                    });
                    setCommentUsers(updatedUsers);
                }

                setComments((prev) => [newCommentObj, ...prev]);
                setTotalComments((prev) => prev + 1);
                setNewComment('');
            }
        } catch (err) {
            console.error('Error posting comment:', err);
        } finally {
            setSubmittingComment(false);
        }
    };

    const handleDeleteComment = async (commentId: string) => {
        if (!user) return;

        const confirmed = window.confirm('Are you sure you want to delete this comment?');
        if (!confirmed) return;

        try {
            const success = await InteractionService.deleteReviewComment(commentId, user.id);

            if (success) {
                // Remove the comment from the list
                setComments((prev) => prev.filter(comment => comment.commentId !== commentId));
                setTotalComments((prev) => prev - 1);
            }
        } catch (err) {
            console.error('Error deleting comment:', err);
        }
    };

    const handleToggleLike = async () => {
        if (!interaction?.review?.reviewId || !user || processingLike) return;

        setProcessingLike(true);

        try {
            if (hasLikedReview) {
                const success = await InteractionService.unlikeReview(
                    interaction.review.reviewId,
                    user.id
                );

                if (success) {
                    setHasLikedReview(false);
                    setLikeCount((prev) => Math.max(0, prev - 1));
                }
            } else {
                const success = await InteractionService.likeReview(
                    interaction.review.reviewId,
                    user.id
                );

                if (success) {
                    setHasLikedReview(true);
                    setLikeCount((prev) => prev + 1);
                }
            }
        } catch (err) {
            console.error('Error toggling like:', err);
        } finally {
            setProcessingLike(false);
        }
    };

    if (loading) {
        return (
            <div className="max-w-4xl mx-auto py-8 px-4">
                <div className="flex justify-center items-center h-64">
                    <div className="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-primary-600"></div>
                    <span className="ml-3 text-lg text-gray-600">Loading interaction...</span>
                </div>
            </div>
        );
    }

    if (error || !interaction || !catalogItem) {
        return (
            <div className="max-w-4xl mx-auto py-8 px-4">
                <div className="bg-red-50 border border-red-200 text-red-700 p-6 rounded-lg">
                    <h2 className="text-xl font-semibold mb-2">Error</h2>
                    <p>{error || "Couldn't find the interaction you're looking for."}</p>
                    <button
                        onClick={() => navigate(-1)}
                        className="mt-4 px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
                    >
                        Go Back
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="max-w-4xl mx-auto py-8 px-4">
            <div className="mb-6">
                <button
                    onClick={() => navigate(-1)}
                    className="text-gray-600 hover:text-gray-900 flex items-center"
                >
                    <svg className="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                    </svg>
                    Back
                </button>
            </div>

            <div className="flex flex-col md:flex-row gap-6 bg-white shadow rounded-lg p-6 mb-8">
                {/* Left column: Item image */}
                <div className="md:w-1/3 flex-shrink-0">
                    <div className="aspect-square w-full rounded-md overflow-hidden shadow-md">
                        <img
                            src={catalogItem.imageUrl || '/placeholder-album.jpg'}
                            alt={catalogItem.name}
                            className="w-full h-full object-cover"
                        />
                    </div>
                </div>

                {/* Right column: Interaction details */}
                <div className="md:w-2/3">
                    {/* Creator info */}
                    <div className="flex items-center mb-4">
                        <Link to={`/people/${creatorProfile.id}`} className="flex items-center">
                            {creatorProfile.avatarUrl ? (
                                <img
                                    src={creatorProfile.avatarUrl}
                                    alt={creatorProfile.name}
                                    className="h-10 w-10 rounded-full object-cover mr-3"
                                />
                            ) : (
                                <div className="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-lg font-bold mr-3">
                                    {creatorProfile.name.charAt(0).toUpperCase()}{creatorProfile.surname.charAt(0).toUpperCase()}
                                </div>
                            )}
                            <div>
                                <span className="font-medium text-gray-900">{creatorProfile.name} {creatorProfile.surname}</span>
                                <span className="text-gray-500 text-sm block">@{creatorProfile.username}</span>
                            </div>
                        </Link>
                    </div>

                    {/* Item info */}
                    <div className="mb-4">
                        <h1 className="text-2xl font-bold text-gray-900 mb-1">
                            <Link
                                to={`/${interaction.itemType.toLowerCase()}/${catalogItem.spotifyId}`}
                                className="hover:text-primary-600"
                            >
                                {catalogItem.name}
                            </Link>
                        </h1>
                        <div className="flex items-center text-gray-600 mb-2">
                            <span className="mr-3">{catalogItem.artistName}</span>
                            <span className="mr-3">
                {catalogItem.releaseDate ? new Date(catalogItem.releaseDate).getFullYear() : ''}
              </span>
                            <span className="bg-gray-200 px-2 py-0.5 rounded-full text-xs">
                {interaction.itemType}
              </span>
                        </div>
                    </div>

                    {/* Interaction details */}
                    <div className="space-y-3 mb-4">
                        {/* Date */}
                        <div className="flex items-center text-gray-600 text-sm">
                            <Calendar className="h-4 w-4 mr-1" />
                            <span>{formatDate(interaction.createdAt)}</span>
                        </div>

                        {/* Rating */}
                        {interaction.rating && (
                            <div className="flex items-center">
                                <NormalizedStarDisplay
                                    currentGrade={interaction.rating.normalizedGrade}
                                    minGrade={1}
                                    maxGrade={10}
                                    size="md"
                                />

                                {interaction.rating.isComplex && (
                                    <button
                                        onClick={() => setIsRatingModalOpen(true)}
                                        className="ml-2 text-primary-600 hover:text-primary-800 focus:outline-none"
                                        title="View detailed rating"
                                    >
                                        <SlidersHorizontal className="h-4 w-4" />
                                    </button>
                                )}
                            </div>
                        )}

                        {/* Like indicator */}
                        {interaction.isLiked && (
                            <div className="flex items-center text-red-500">
                                <Heart className="h-5 w-5 fill-red-500 mr-1" />
                                <span>Liked</span>
                            </div>
                        )}
                    </div>

                    {/* Review */}
                    {interaction.review && (
                        <div className="mt-6 space-y-3">
                            <div className="bg-gray-50 p-4 rounded-md border border-gray-200">
                                <p className="text-gray-800 whitespace-pre-wrap">{interaction.review.reviewText}</p>
                            </div>

                            {/* Review actions */}
                            <div className="flex items-center space-x-4">
                                {isAuthenticated && (
                                    <button
                                        onClick={handleToggleLike}
                                        disabled={processingLike}
                                        className={`flex items-center text-sm ${
                                            hasLikedReview ? 'text-primary-600' : 'text-gray-500 hover:text-primary-600'
                                        }`}
                                    >
                                        <ThumbsUp className={`h-4 w-4 mr-1 ${hasLikedReview ? 'fill-primary-600' : ''}`} />
                                        <span>{hasLikedReview ? 'Liked' : 'Like'}</span>
                                        {likeCount > 0 && <span className="ml-1">({likeCount})</span>}
                                    </button>
                                )}

                                <button
                                    onClick={() => document.getElementById('comments-section')?.scrollIntoView({ behavior: 'smooth' })}
                                    className="flex items-center text-sm text-gray-500 hover:text-primary-600"
                                >
                                    <MessageSquare className="h-4 w-4 mr-1" />
                                    <span>Comments</span>
                                    {totalComments > 0 && <span className="ml-1">({totalComments})</span>}
                                </button>

                                {user && user.id === interaction.userId && (
                                    <button
                                        onClick={() => {
                                            alert('Delete functionality to be implemented');
                                        }}
                                        className="flex items-center text-sm text-red-500 hover:text-red-700"
                                    >
                                        <Trash2 className="h-4 w-4 mr-1" />
                                        <span>Delete</span>
                                    </button>
                                )}

                                {isAuthenticated && user?.id !== interaction.userId && (
                                    <button
                                        onClick={() => {
                                            alert('Report functionality to be implemented');
                                        }}
                                        className="flex items-center text-sm text-gray-500 hover:text-gray-700"
                                    >
                                        <Flag className="h-4 w-4 mr-1" />
                                        <span>Report</span>
                                    </button>
                                )}
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* Comments section */}
            {interaction.review && (
                <div id="comments-section" className="bg-white shadow rounded-lg p-6">
                    <h2 className="text-xl font-bold mb-6 flex items-center">
                        <MessageSquare className="h-5 w-5 mr-2 text-gray-500" />
                        Comments
                        {totalComments > 0 && <span className="ml-2 text-gray-500">({totalComments})</span>}
                    </h2>

                    {/* Comment form */}
                    {isAuthenticated ? (
                        <div className="mb-6">
                            <div className="flex">
                                <div className="mr-3 flex-shrink-0">
                                    {user?.avatarUrl ? (
                                        <img
                                            src={user.avatarUrl}
                                            alt={user.name}
                                            className="h-10 w-10 rounded-full object-cover"
                                        />
                                    ) : (
                                        <div className="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-lg font-bold">
                                            {user?.name.charAt(0).toUpperCase()}{user?.surname.charAt(0).toUpperCase()}
                                        </div>
                                    )}
                                </div>
                                <div className="flex-grow relative">
                  <textarea
                      value={newComment}
                      onChange={(e) => setNewComment(e.target.value)}
                      placeholder="Write a comment..."
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                      rows={3}
                  />
                                    <button
                                        onClick={handleSubmitComment}
                                        disabled={!newComment.trim() || submittingComment}
                                        className="absolute bottom-3 right-3 p-1 rounded-full bg-primary-500 text-white disabled:bg-gray-300 disabled:cursor-not-allowed"
                                    >
                                        <Send className="h-4 w-4" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    ) : (
                        <div className="mb-6 bg-gray-50 p-4 rounded-md text-center">
                            <p className="text-gray-600 mb-2">You need to be logged in to comment</p>
                            <button
                                onClick={() => navigate('/login', { state: { from: `/interaction/${id}` } })}
                                className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
                            >
                                Log In
                            </button>
                        </div>
                    )}

                    {/* Comments list */}
                    <div className="space-y-4">
                        {commentsError && (
                            <div className="bg-red-50 border border-red-200 text-red-700 p-4 rounded-md">
                                {commentsError}
                            </div>
                        )}

                        {comments.length === 0 && !commentsLoading && !commentsError ? (
                            <div className="text-center py-8 text-gray-500">
                                No comments yet. Be the first to comment!
                            </div>
                        ) : (
                            <div className="divide-y divide-gray-200">
                                {comments.map((comment) => {
                                    const commentUser = commentUsers.get(comment.userId);
                                    return (
                                        <div key={comment.commentId} className="py-4">
                                            <div className="flex">
                                                <div className="mr-3 flex-shrink-0">
                                                    <Link to={`/people/${comment.userId}`}>
                                                        {commentUser?.avatarUrl ? (
                                                            <img
                                                                src={commentUser.avatarUrl}
                                                                alt={`${commentUser.name}`}
                                                                className="h-10 w-10 rounded-full object-cover"
                                                            />
                                                        ) : (
                                                            <div className="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-lg font-bold">
                                                                {commentUser ? (
                                                                    `${commentUser.name.charAt(0)}${commentUser.surname.charAt(0)}`
                                                                ) : (
                                                                    '?'
                                                                )}
                                                            </div>
                                                        )}
                                                    </Link>
                                                </div>
                                                <div className="flex-grow">
                                                    <div className="flex justify-between mb-1">
                                                        <div>
                                                            <Link
                                                                to={`/people/${comment.userId}`}
                                                                className="font-medium text-gray-900 hover:text-primary-600"
                                                            >
                                                                {commentUser ? (
                                                                    <>
                                                                        {commentUser.name} {commentUser.surname}
                                                                        {commentUser.username && (
                                                                            <span className="text-gray-500 text-sm ml-1">
                                        @{commentUser.username}
                                      </span>
                                                                        )}
                                                                    </>
                                                                ) : (
                                                                    'User'
                                                                )}
                                                            </Link>
                                                            <span className="text-gray-500 text-sm ml-2">
                                {formatDate(comment.commentedAt)}
                              </span>
                                                        </div>
                                                        {user && user.id === comment.userId && (
                                                            <button
                                                                onClick={() => handleDeleteComment(comment.commentId)}
                                                                className="text-gray-400 hover:text-red-500"
                                                                title="Delete comment"
                                                            >
                                                                <Trash2 className="h-4 w-4" />
                                                            </button>
                                                        )}
                                                    </div>
                                                    <p className="text-gray-800">{comment.commentText}</p>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}

                        {/* Load more comments button */}
                        {hasMoreComments && (
                            <div className="mt-4 text-center">
                                <button
                                    onClick={handleLoadMoreComments}
                                    disabled={commentsLoading}
                                    className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                                >
                                    {commentsLoading ? (
                                        <>
                                            <span className="inline-block h-4 w-4 border-t-2 border-b-2 border-primary-600 rounded-full animate-spin mr-2 align-middle"></span>
                                            Loading...
                                        </>
                                    ) : (
                                        'Load More Comments'
                                    )}
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            )}

            {/* Complex Rating Modal */}
            {interaction.rating?.isComplex && interaction.rating?.ratingId && (
                <ComplexRatingModal
                    isOpen={isRatingModalOpen}
                    onClose={() => setIsRatingModalOpen(false)}
                    ratingId={interaction.rating.ratingId}
                    itemName={catalogItem.name}
                    artistName={catalogItem.artistName}
                    date={formatDate(interaction.createdAt)}
                />
            )}
        </div>
    );
};

export default InteractionDetailPage;