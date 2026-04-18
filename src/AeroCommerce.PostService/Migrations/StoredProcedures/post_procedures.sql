-- ════════════════════════════════════════════════════════
--  PostService — Stored Procedures / Functions
--  Dùng cho: Supabase SQL Editor hoặc Migration seed
-- ════════════════════════════════════════════════════════

-- ── SP: Full-text search bài viết ──────────────────────
CREATE OR REPLACE FUNCTION sp_search_posts(p_keyword TEXT)
RETURNS SETOF posts AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM posts
    WHERE status = 'Published'
      AND (
          to_tsvector('english', title || ' ' || content)
          @@ plainto_tsquery('english', p_keyword)
      )
    ORDER BY created_at DESC;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Lấy bài viết trending (mới nhất đã publish) ────
CREATE OR REPLACE FUNCTION sp_get_trending_posts(p_limit INT DEFAULT 10)
RETURNS SETOF posts AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM posts
    WHERE status = 'Published'
    ORDER BY created_at DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Archive toàn bộ bài của một author ─────────────
CREATE OR REPLACE PROCEDURE sp_archive_author_posts(p_author_id UUID)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE posts
    SET status     = 'Archived',
        updated_at = NOW()
    WHERE author_id = p_author_id
      AND status    = 'Published';
END;
$$;

-- ── SP: Thống kê posts ──────────────────────────────────
CREATE OR REPLACE FUNCTION sp_get_post_stats()
RETURNS TABLE(
    total_posts    BIGINT,
    published      BIGINT,
    draft          BIGINT,
    archived       BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)                                             AS total_posts,
        COUNT(*) FILTER (WHERE status = 'Published')        AS published,
        COUNT(*) FILTER (WHERE status = 'Draft')            AS draft,
        COUNT(*) FILTER (WHERE status = 'Archived')         AS archived
    FROM posts;
END;
$$ LANGUAGE plpgsql;

-- ── Index hỗ trợ full-text search ──────────────────────
CREATE INDEX IF NOT EXISTS idx_posts_fts
    ON posts USING GIN (to_tsvector('english', title || ' ' || content));
